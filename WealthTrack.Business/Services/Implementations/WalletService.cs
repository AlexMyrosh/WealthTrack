using AutoMapper;
using Microsoft.Extensions.Configuration;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class WalletService(IUnitOfWork unitOfWork, IMapper mapper, ITransactionService transactionService, IConfiguration configuration) : IWalletService
    {
        private readonly string _balanceCorrectionCategoryId = configuration["SystemCategories:BalanceCorrectionId"] ?? throw new InvalidOperationException("Unable to get balance correction category id from configuration");

        public async Task<Guid> CreateAsync(WalletUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!model.Balance.HasValue)
            {
                throw new ArgumentException("Balance should not be null or empty");
            }
            
            if (!model.IsPartOfGeneralBalance.HasValue)
            {
                throw new ArgumentException("IsPartOfGeneralBalance should not be null or empty");
            }
            
            if (!model.Type.HasValue)
            {
                throw new ArgumentException("Type should not be null or empty");
            }
            
            if (!Enum.IsDefined(typeof(WalletType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
            }

            var domainModel = mapper.Map<Wallet>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            domainModel.Status = EntityStatus.Active;
            var createdEntityId = await unitOfWork.WalletRepository.CreateAsync(domainModel);
            // Move this logic to Event handlers
            if (model.Balance.HasValue && model.Balance != 0)
            {
                await unitOfWork.TransactionRepository.CreateAsync(new Transaction
                {
                    Amount = decimal.Abs(model.Balance.Value),
                    Description = "Balance correction",
                    CreatedDate = DateTimeOffset.Now,
                    CategoryId = new Guid(_balanceCorrectionCategoryId),
                    Type = model.Balance.Value > 0 ? TransactionType.Income : TransactionType.Expense,
                    WalletId = createdEntityId
                });
            }
            
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<WalletDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModel = await unitOfWork.WalletRepository.GetByIdAsync(id, include);
            var result = mapper.Map<WalletDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<WalletDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.WalletRepository.GetAllAsync(include);
            var result = mapper.Map<List<WalletDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, WalletUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }
            
            if (model.Type.HasValue)
            {
                throw new ArgumentException("Type update is not allowed");
            }
            
            if (model.BudgetId.HasValue)
            {
                throw new ArgumentException("Budget update is not allowed");
            }

            var originalModel = await unitOfWork.WalletRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {id.ToString()}");
            }

            // Move this logic to Event handlers
            if (model.Balance.HasValue && model.Balance != originalModel.Balance)
            {
                await unitOfWork.TransactionRepository.CreateAsync(new Transaction
                {
                    Amount = decimal.Abs(originalModel.Balance - model.Balance.Value),
                    Description = "Balance correction",
                    CreatedDate = DateTimeOffset.Now,
                    CategoryId = new Guid(_balanceCorrectionCategoryId),
                    Type = model.Balance.Value > originalModel.Balance ? TransactionType.Income : TransactionType.Expense,
                    WalletId = id
                });
            }

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.WalletRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id, bool shouldBeSaved = true)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.WalletRepository.GetByIdAsync(id, $"{nameof(Wallet.Transactions)},{nameof(Wallet.IncomeTransferTransactions)},{nameof(Wallet.OutgoingTransferTransactions)}");
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {id.ToString()}");
            }
            
            var transactionIdsToDelete = domainModelToDelete.Transactions.Select(t => t.Id).ToList();
            var outgoingTransactionIdsToDelete = domainModelToDelete.OutgoingTransferTransactions.Select(t => t.Id).ToList();
            var incomeTransactionIdsToDelete = domainModelToDelete.IncomeTransferTransactions.Select(t => t.Id).ToList();
            if (transactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(transactionIdsToDelete, false);
            }
            
            if (outgoingTransactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(outgoingTransactionIdsToDelete, false);
            }
            
            if (incomeTransactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(incomeTransactionIdsToDelete, false);
            }
            
            await unitOfWork.WalletRepository.HardDeleteAsync(domainModelToDelete);
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }
        
        public async Task BulkHardDeleteAsync(List<Guid> ids, bool shouldBeSaved = true)
        {
            if (ids.Any(id => id == Guid.Empty))
            {
                throw new ArgumentException("One or more IDs are empty");
            }

            var domainModelsToDelete = await unitOfWork.WalletRepository.GetByIdsAsync(ids, 
                $"{nameof(Wallet.Transactions)}," +
                $"{nameof(Wallet.IncomeTransferTransactions)}," +
                $"{nameof(Wallet.OutgoingTransferTransactions)}");
            if (domainModelsToDelete is null || domainModelsToDelete.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get wallets from database by ids: {string.Join(", ", ids)}");
            }
            
            var transactionIdsToDelete = domainModelsToDelete.SelectMany(w => w.Transactions).Select(t => t.Id).ToList();
            var outgoingTransactionIdsToDelete = domainModelsToDelete.SelectMany(w => w.OutgoingTransferTransactions).Select(t => t.Id).ToList();
            var incomeTransactionIdsToDelete = domainModelsToDelete.SelectMany(w => w.IncomeTransferTransactions).Select(t => t.Id).ToList();
            if (transactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(transactionIdsToDelete, false);
            }
            
            if (outgoingTransactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(outgoingTransactionIdsToDelete, false);
            }
            
            if (incomeTransactionIdsToDelete.Count != 0)
            {
                await transactionService.BulkHardDeleteAsync(incomeTransactionIdsToDelete, false);
            }
            
            unitOfWork.WalletRepository.BulkHardDelete(domainModelsToDelete);
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }

        public async Task ArchiveAsync(Guid id, bool shouldBeSaved = true)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToArchive = await unitOfWork.WalletRepository.GetByIdAsync(id, $"{nameof(Wallet.Transactions)},{nameof(Wallet.IncomeTransferTransactions)},{nameof(Wallet.OutgoingTransferTransactions)}");
            if (domainModelToArchive is null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {id.ToString()}");
            }
            
            var transactionIdsToArchive = domainModelToArchive.Transactions.Select(t => t.Id).ToList();
            var outgoingTransactionIdsToArchive = domainModelToArchive.OutgoingTransferTransactions.Select(t => t.Id).ToList();
            var incomeTransactionIdsToArchive = domainModelToArchive.IncomeTransferTransactions.Select(t => t.Id).ToList();
            if (transactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(transactionIdsToArchive, false);
            }
            
            if (outgoingTransactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(outgoingTransactionIdsToArchive, false);
            }
            
            if (incomeTransactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(incomeTransactionIdsToArchive, false);
            }

            domainModelToArchive.Status = EntityStatus.Archived;
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }

        public async Task BulkArchiveAsync(List<Guid> ids, bool shouldBeSaved = true)
        {
            if (ids.Any(id => id == Guid.Empty))
            {
                throw new ArgumentException("One or more IDs are empty");
            }

            var domainModelsToArchive = await unitOfWork.WalletRepository.GetByIdsAsync(ids, 
                $"{nameof(Wallet.Transactions)}," +
                $"{nameof(Wallet.IncomeTransferTransactions)}," +
                $"{nameof(Wallet.OutgoingTransferTransactions)}");
            if (domainModelsToArchive is null || domainModelsToArchive.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get wallets from database by ids: {string.Join(", ", ids)}");
            }
            
            var transactionIdsToArchive = domainModelsToArchive.SelectMany(w => w.Transactions).Select(t => t.Id).ToList();
            var outgoingTransactionIdsToArchive = domainModelsToArchive.SelectMany(w => w.OutgoingTransferTransactions).Select(t => t.Id).ToList();
            var incomeTransactionIdsToArchive = domainModelsToArchive.SelectMany(w => w.IncomeTransferTransactions).Select(t => t.Id).ToList();
            if (transactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(transactionIdsToArchive, false);
            }
            
            if (outgoingTransactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(outgoingTransactionIdsToArchive, false);
            }
            
            if (incomeTransactionIdsToArchive.Count != 0)
            {
                await transactionService.BulkArchiveAsync(incomeTransactionIdsToArchive, false);
            }
            
            domainModelsToArchive.ForEach(w => w.Status = EntityStatus.Archived);
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }
    }
}
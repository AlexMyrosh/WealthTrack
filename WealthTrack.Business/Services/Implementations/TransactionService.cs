using AutoMapper;
using Microsoft.Extensions.Configuration;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using Transaction = WealthTrack.Data.DomainModels.Transaction;

namespace WealthTrack.Business.Services.Implementations
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher) : ITransactionService
    {
        public async Task<Guid> CreateAsync(TransactionUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.CategoryId.HasValue)
            {
                var category = await unitOfWork.CategoryRepository.GetByIdAsync(model.CategoryId.Value);
                if (category is null)
                {
                    throw new ArgumentException($"Category with id {model.CategoryId} not found");    
                }
                
                if (model.Type != category.Type)
                {
                    throw new ArgumentException("Transaction type is not aligned with the category's type");
                }
            }

            if (!model.Amount.HasValue || model.Amount.Value < 0)
            {
                throw new ArgumentException("Amount value is not correct");
            }

            if (!model.TransactionDate.HasValue)
            {
                throw new ArgumentException("TransactionDate value is missing");
            }

            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            var transactionCreatedEventModel = mapper.Map<TransactionCreatedEvent>(domainModel);
            await eventPublisher.PublishAsync(transactionCreatedEventModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<Guid> CreateAsync(TransferTransactionUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            if (!model.Amount.HasValue || model.Amount.Value < 0)
            {
                throw new ArgumentException("Amount value is not correct");
            }
            
            if (!model.TransactionDate.HasValue)
            {
                throw new ArgumentException("TransactionDate value is missing");
            }

            var domainModel = mapper.Map<TransferTransaction>(model);
            if (!await IsWalletsHaveTheSameBudget(domainModel.SourceWalletId, domainModel.TargetWalletId))
            {
                throw new ArgumentException("Source and Target wallets are from different budgets");
            }
            
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            var createdEntityId = await unitOfWork.TransferTransactionRepository.CreateAsync(domainModel);
            var transferTransactionCreatedEventModel = mapper.Map<TransferTransactionCreatedEvent>(domainModel);
            await eventPublisher.PublishAsync(transferTransactionCreatedEventModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<TransactionDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty.", nameof(id));
            }

            var regular = await unitOfWork.TransactionRepository.GetByIdAsync(id, include);
            if (regular != null)
            {
                return mapper.Map<TransactionDetailsBusinessModel>(regular);
            }

            var transfer = await unitOfWork.TransferTransactionRepository.GetByIdAsync(id, include);
            if (transfer != null)
            {
                return mapper.Map<TransactionDetailsBusinessModel>(transfer);
            }

            return null;
        }

        public async Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var transactionDomainModels = await unitOfWork.TransactionRepository.GetAllAsync(include);
            var transferTransactionDomainModels = await unitOfWork.TransferTransactionRepository.GetAllAsync(include);
            var transactionBusinessModels = mapper.Map<List<TransactionDetailsBusinessModel>>(transactionDomainModels);
            var transferTransactionBusinessModels = mapper.Map<List<TransactionDetailsBusinessModel>>(transferTransactionDomainModels);
            var result = transactionBusinessModels
                .Concat(transferTransactionBusinessModels)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
            return result;
        }

        public async Task UpdateAsync(Guid id, TransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            if (model.Type.HasValue)
            {
                throw new ArgumentException("Transaction type is not allowed to be updated");
            }

            if (model.Amount is < 0)
            {
                throw new ArgumentException("Amount value is not correct");
            }

            if (model.WalletId.HasValue && await unitOfWork.WalletRepository.GetByIdAsync(model.WalletId.Value) == null)
            {
                throw new  ArgumentException($"Wallet with id {model.WalletId.Value} not found");
            }

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, "Wallet");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }
            
            if (model.WalletId.HasValue && !await IsWalletsHaveTheSameBudget(model.WalletId.Value, originalModel.WalletId))
            {
                throw new ArgumentException("Source and Target wallets are from different budgets");
            }
            
            await eventPublisher.PublishAsync(new TransactionUpdatedEvent
            {
                CategoryId_Old = originalModel.CategoryId,
                CategoryId_New = model.CategoryId,
                TransactionType_Old = originalModel.Type,
                TransactionType_New = model.Type,
                WalletId_Old = originalModel.WalletId,
                WalletId_New = model.WalletId,
                Amount_Old = originalModel.Amount,
                Amount_New = model.Amount,
                TransactionDate_Old = originalModel.TransactionDate,
                TransactionDate_New = model.TransactionDate,
            });
            
            mapper.Map(model, originalModel);
            if (model.CategoryId.HasValue)
            {
                var category = await unitOfWork.CategoryRepository.GetByIdAsync(model.CategoryId.Value);
                if (category is null)
                {
                    throw new ArgumentException($"Category with id {model.CategoryId} not found");    
                }
                
                if (originalModel.Type != category.Type)
                {
                    throw new ArgumentException("Transaction type is not aligned with the category's type");
                }
            }
            
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, TransferTransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            if (model.SourceWalletId.HasValue &&  await unitOfWork.WalletRepository.GetByIdAsync(model.SourceWalletId.Value) == null)
            {
                throw new ArgumentException(nameof(model.SourceWalletId));
            }
            
            if (model.TargetWalletId.HasValue &&  await unitOfWork.WalletRepository.GetByIdAsync(model.TargetWalletId.Value) == null)
            {
                throw new ArgumentException(nameof(model.TargetWalletId));
            }
            
            if (model.Amount is < 0)
            {
                throw new ArgumentException("Amount value is not correct");
            }

            var originalModel = await unitOfWork.TransferTransactionRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }
            
            await eventPublisher.PublishAsync(new TransferTransactionUpdatedEvent
            {
                Amount_New = model.Amount,
                Amount_Old = originalModel.Amount,
                SourceWalletId_New = model.SourceWalletId,
                SourceWalletId_Old = originalModel.SourceWalletId,
                TargetWalletId_New = model.TargetWalletId,
                TargetWalletId_Old = originalModel.TargetWalletId,
            });
            
            mapper.Map(model, originalModel);
            if (!await IsWalletsHaveTheSameBudget(originalModel.SourceWalletId, originalModel.TargetWalletId))
            {
                throw new ArgumentException("Source and Target wallets are from different budgets");
            }
            
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.TransferTransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task UnassignCategoryAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }
            
            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, $"{nameof(Transaction.Category)}");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }
            
            await eventPublisher.PublishAsync(new TransactionUpdatedEvent
            {
                CategoryId_Old = originalModel.CategoryId,
                CategoryId_New = null,
                IsCategoryDeleted = true,
                TransactionType_Old = originalModel.Type,
                TransactionType_New = null,
                WalletId_Old = originalModel.WalletId,
                WalletId_New = null,
                Amount_Old = originalModel.Amount,
                Amount_New = null,
                TransactionDate_Old = originalModel.TransactionDate,
                TransactionDate_New = null
            });

            originalModel.CategoryId = null;
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (domainModelToDelete is not null)
            {
                unitOfWork.TransactionRepository.HardDelete(domainModelToDelete);
                var transactionDeletedEventModel = mapper.Map<TransactionDeletedEvent>(domainModelToDelete);
                await eventPublisher.PublishAsync(transactionDeletedEventModel);
            }
            else
            {
                var transferTransactionDomainModelToDelete = await unitOfWork.TransferTransactionRepository.GetByIdAsync(id);
                if (transferTransactionDomainModelToDelete is not null)
                {
                    unitOfWork.TransferTransactionRepository.HardDelete(transferTransactionDomainModelToDelete);
                    var transferTransactionDeletedEventModel = mapper.Map<TransferTransactionDeletedEvent>(transferTransactionDomainModelToDelete);
                    await eventPublisher.PublishAsync(transferTransactionDeletedEventModel);
                }
                else
                {
                    throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
                }
            }
            
            await unitOfWork.SaveAsync();
        }

        private async Task<bool> IsWalletsHaveTheSameBudget(Guid walletId1, Guid walletId2)
        {
            var wallet1 = await unitOfWork.WalletRepository.GetByIdAsync(walletId1);
            var wallet2 = await unitOfWork.WalletRepository.GetByIdAsync(walletId2);
            if (wallet1 is null)
            {
                throw new ArgumentException($"Unable to get wallet by id - {walletId1}");
            }
            
            if (wallet2 is null)
            {
                throw new ArgumentException($"Unable to get wallet by id - {walletId1}");
            }

            if (wallet1.Id == wallet2.Id)
            {
                throw new ArgumentException("Wallets are the same");
            }
            
            return wallet1.BudgetId == wallet2.BudgetId;
        }
    }
}
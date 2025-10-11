using AutoMapper;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;
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
                
                if ((model.Type == TransactionType.Income && category.Type != OperationType.Income) ||
                    (model.Type == TransactionType.Expense && category.Type != OperationType.Expense))
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
            
            if (!model.Type.HasValue || !Enum.IsDefined(typeof(TransactionType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
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

            if (!model.SourceWalletId.HasValue || model.SourceWalletId == Guid.Empty)
            {
                throw new ArgumentException("SourceWalletId value is missing or empty");
            }
            
            if (!model.TargetWalletId.HasValue || model.TargetWalletId == Guid.Empty)
            {
                throw new ArgumentException("TargetWalletId value is missing or empty");
            }

            if (model.SourceWalletId == model.TargetWalletId)
            {
                throw new ArgumentException("Target and Source Wallets are the same");
            }

            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
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
            var result = mapper.Map<TransactionDetailsBusinessModel>(regular);
            return result;
        }

        public async Task<int> GetCountAsync()
        {
            var result = await unitOfWork.TransactionRepository.GetCountAsync();
            return result;
        }

        public async Task<List<TransactionDetailsBusinessModel>> GetPageAsync(int pageNumber, int pageSize, string include = "")
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentException("Page number must be greater than zero");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be greater than zero");
            }
            
            var domainModel = await unitOfWork.TransactionRepository.GetPageAsync(pageNumber, pageSize, include);
            var result = mapper.Map<List<TransactionDetailsBusinessModel>>(domainModel);
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
            
            if (model.Type.HasValue && !Enum.IsDefined(typeof(TransactionType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
            }

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, "Wallet");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
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
                
                if ((originalModel.Type == TransactionType.Income && category.Type != OperationType.Income) ||
                    (originalModel.Type == TransactionType.Expense && category.Type != OperationType.Expense))
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

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

            var transferUpdatedEventModel = new TransferTransactionUpdatedEvent
            {
                Amount_New = model.Amount,
                Amount_Old = originalModel.Amount,
                SourceWalletId_New = model.SourceWalletId,
                SourceWalletId_Old = originalModel.SourceWalletId.Value,
                TargetWalletId_New = model.TargetWalletId,
                TargetWalletId_Old = originalModel.TargetWalletId.Value,
            };
            mapper.Map(model, originalModel);
            if (originalModel.SourceWalletId == originalModel.TargetWalletId)
            {
                throw new ArgumentException("Source and Target wallets are the same");
            }
            
            await eventPublisher.PublishAsync(transferUpdatedEventModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.TransactionRepository.Update(originalModel);
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

            if (originalModel.Type == TransactionType.Transfer)
            {
                throw new ArgumentException("This operation is not supported for transfer transactions");
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

        public async Task HardDeleteAsync(Guid id, bool shouldBeSaved = true)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }
            
            unitOfWork.TransactionRepository.HardDelete(domainModelToDelete);
            if (domainModelToDelete.Type == TransactionType.Transfer)
            {
                var transactionArchivedEventModel = mapper.Map<TransferTransactionDeletedEvent>(domainModelToDelete);
                await eventPublisher.PublishAsync(transactionArchivedEventModel);
            }
            else
            {
                var transactionArchivedEventModel = mapper.Map<TransactionDeletedEvent>(domainModelToDelete);
                await eventPublisher.PublishAsync(transactionArchivedEventModel);
            }
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

            var transactionDomainModelsToDelete = await unitOfWork.TransactionRepository.GetByIdsAsync(ids);
            if (transactionDomainModelsToDelete.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get transactions from database by ids: {string.Join(", ", ids)}");
            }
            
            unitOfWork.TransactionRepository.BulkHardDelete(transactionDomainModelsToDelete);
            foreach (var transactionDomainModelToDelete in transactionDomainModelsToDelete)
            {
                if (transactionDomainModelToDelete.Type == TransactionType.Transfer)
                {
                    var transactionArchivedEventModel = mapper.Map<TransferTransactionDeletedEvent>(transactionDomainModelToDelete);
                    await eventPublisher.PublishAsync(transactionArchivedEventModel);
                }
                else
                {
                    var transactionArchivedEventModel = mapper.Map<TransactionDeletedEvent>(transactionDomainModelToDelete);
                    await eventPublisher.PublishAsync(transactionArchivedEventModel);
                }
            }
            
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

            var domainModelToArchive = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (domainModelToArchive is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }
            
            domainModelToArchive.Status = EntityStatus.Archived;
            if (domainModelToArchive.Type == TransactionType.Transfer)
            {
                var transactionArchivedEventModel = mapper.Map<TransferTransactionDeletedEvent>(domainModelToArchive);
                await eventPublisher.PublishAsync(transactionArchivedEventModel);
            }
            else
            {
                var transactionArchivedEventModel = mapper.Map<TransactionDeletedEvent>(domainModelToArchive);
                await eventPublisher.PublishAsync(transactionArchivedEventModel);
            }
            
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

            var transactionDomainModelsToArchive = await unitOfWork.TransactionRepository.GetByIdsAsync(ids);
            
            if (transactionDomainModelsToArchive.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get transactions from database by ids: {string.Join(", ", ids)}");
            }
            
            transactionDomainModelsToArchive.ForEach(t => t.Status = EntityStatus.Archived);
            foreach (var transactionDomainModelToArchive in transactionDomainModelsToArchive)
            {
                if (transactionDomainModelToArchive.Type == TransactionType.Transfer)
                {
                    var transactionArchivedEventModel = mapper.Map<TransferTransactionDeletedEvent>(transactionDomainModelToArchive);
                    await eventPublisher.PublishAsync(transactionArchivedEventModel);
                }
                else
                {
                    var transactionArchivedEventModel = mapper.Map<TransactionDeletedEvent>(transactionDomainModelToArchive);
                    await eventPublisher.PublishAsync(transactionArchivedEventModel);
                }
            }
            
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }
    }
}
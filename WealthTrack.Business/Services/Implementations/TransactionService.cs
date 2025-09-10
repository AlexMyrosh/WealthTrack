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

            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
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

            var domainModel = mapper.Map<TransferTransaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
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
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, include);
            var result = mapper.Map<TransactionDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.TransactionRepository.GetAllAsync(include);
            var result = mapper.Map<List<TransactionDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, TransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
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
            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, TransferTransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
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
            unitOfWork.TransferTransactionRepository.Update(originalModel);
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
    }
}
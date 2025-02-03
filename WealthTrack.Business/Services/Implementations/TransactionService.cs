using AutoMapper;
using Microsoft.Extensions.Configuration;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;
using Transaction = WealthTrack.Data.DomainModels.Transaction;

namespace WealthTrack.Business.Services.Implementations
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher, IConfiguration configuration) : ITransactionService
    {
        private readonly string _transferCategoryId = configuration["SystemCategories:TransferId"] ?? throw new InvalidOperationException("Unable to get transfer category id from configuration");

        public async Task<Guid> CreateAsync(TransactionUpsertBusinessModel model)
        {
            // TODO: Need to add check that model is not for transfer
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            await eventPublisher.PublishAsync(new TransactionCreatedEvent
            {

            });

            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<Guid> CreateAsync(TransferTransactionUpsertBusinessModel model)
        {
            // TODO: Need to add check that model is not for regular transaction
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CategoryId = new Guid(_transferCategoryId);
            domainModel.Type = TransactionType.Transfer;
            domainModel.CreatedDate = DateTimeOffset.Now;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            await eventPublisher.PublishAsync(new TransferTransactionCreatedEvent
            {

            });

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

            if (originalModel.Type == TransactionType.Transfer)
            {
                throw new InvalidOperationException("Unable to update transaction as its type is transfer");
            }

            mapper.Map(model, originalModel);
            unitOfWork.TransactionRepository.Update(originalModel);
            await eventPublisher.PublishAsync(new TransactionUpdatedEvent
            {

            });

            await unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, TransferTransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

            if (originalModel.Type != TransactionType.Transfer)
            {
                throw new InvalidOperationException("Unable to update transaction as its type is not transfer");
            }

            mapper.Map(model, originalModel);
            unitOfWork.TransactionRepository.Update(originalModel);
            await eventPublisher.PublishAsync(new TransferTransactionUpdatedEvent
            {

            });

            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
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
                await eventPublisher.PublishAsync(new TransferTransactionDeletedEvent
                {

                });
            }
            else
            {
                await eventPublisher.PublishAsync(new TransactionDeletedEvent
                {

                });
            }

            await unitOfWork.SaveAsync();
        }
    }
}
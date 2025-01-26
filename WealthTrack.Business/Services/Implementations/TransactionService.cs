using AutoMapper;
using WealthTrack.Business.BusinessModels.Transaction;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;
using Transaction = WealthTrack.Data.DomainModels.Transaction;

namespace WealthTrack.Business.Services.Implementations
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher) : ITransactionService
    {
        public async Task<Guid> CreateAsync(TransactionUpsertBusinessModel model)
        {
            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
            if (model.WalletId.HasValue && model.Type.HasValue && model.Amount.HasValue && model.TransactionDate.HasValue)
            {
                TransactionAddedEvent transactionEvent = new(model.Type.Value, model.Amount.Value, model.WalletId.Value, model.CategoryId, model.TransactionDate.Value);
                await eventPublisher.PublishAsync(transactionEvent);
            }

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
            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, "Wallet");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

            TransactionUpdatedEvent transactionEvent = new(originalModel.CategoryId, model.CategoryId, originalModel.Type, model.Type, originalModel.WalletId, model.WalletId, originalModel.Amount, model.Amount, originalModel.TransactionDate, model.TransactionDate);
            await eventPublisher.PublishAsync(transactionEvent);

            mapper.Map(model, originalModel);

            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.TransactionRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            TransactionDeletedEvent transactionEvent = new(deletedDomainModel.Type, deletedDomainModel.Amount, deletedDomainModel.WalletId, deletedDomainModel.CategoryId, deletedDomainModel.TransactionDate);
            await eventPublisher.PublishAsync(transactionEvent);

            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
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
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

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

        public async Task<Guid> CreateAsync(TransferTransactionUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // TODO: Add event handlers to update wallet and budget balances after this transaction
            var domainModel = mapper.Map<Transaction>(model);
            domainModel.CategoryId = new Guid(_transferCategoryId);
            domainModel.Type = TransactionType.Transfer;
            domainModel.CreatedDate = DateTimeOffset.Now;
            var createdEntityId = await unitOfWork.TransactionRepository.CreateAsync(domainModel);
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
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id, "Wallet");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

            TransactionUpdatedEvent transactionEvent = new(originalModel.CategoryId, model.CategoryId, originalModel.Type, model.Type, originalModel.WalletId.Value, model.WalletId, originalModel.Amount, model.Amount, originalModel.TransactionDate, model.TransactionDate);
            await eventPublisher.PublishAsync(transactionEvent);
            mapper.Map(model, originalModel);
            unitOfWork.TransactionRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, TransferTransactionUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var originalModel = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

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

            var domainModelToDelete = await unitOfWork.TransactionRepository.GetByIdAsync(id);
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get transaction from database by id - {id.ToString()}");
            }

            if ((domainModelToDelete.Type == TransactionType.Income || domainModelToDelete.Type == TransactionType.Expense) && domainModelToDelete.WalletId.HasValue)
            {
                TransactionDeletedEvent transactionEvent = new(domainModelToDelete.Type, domainModelToDelete.Amount, domainModelToDelete.WalletId.Value, domainModelToDelete.CategoryId, domainModelToDelete.TransactionDate);
                await eventPublisher.PublishAsync(transactionEvent);
            }

            unitOfWork.TransactionRepository.HardDelete(domainModelToDelete);
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
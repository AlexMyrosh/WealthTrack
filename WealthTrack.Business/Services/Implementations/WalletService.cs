using AutoMapper;
using Microsoft.Extensions.Configuration;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class WalletService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher, IConfiguration configuration) : IWalletService
    {
        private readonly string _balanceCorrectionCategoryId = configuration["SystemCategories:BalanceCorrectionId"] ?? throw new InvalidOperationException("Unable to get balance correction category id from configuration");

        public async Task<Guid> CreateAsync(WalletUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var domainModel = mapper.Map<Wallet>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            domainModel.Status = WalletStatus.Active;
            var createdEntityId = await unitOfWork.WalletRepository.CreateAsync(domainModel);
            var walletCreatedEventModel = mapper.Map<WalletCreatedEvent>(domainModel);
            await eventPublisher.PublishAsync(walletCreatedEventModel);
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

            var originalModel = await unitOfWork.WalletRepository.GetByIdAsync(id);
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {id.ToString()}");
            }

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

            await eventPublisher.PublishAsync(new WalletUpdatedEvent
            {
                WalletId = id,
                BudgetId_Old = originalModel.BudgetId,
                BudgetId_New = model.BudgetId,
                Balance_Old = originalModel.Balance,
                Balance_New = model.Balance,
                IsPartOfGeneralBalance_Old = originalModel.IsPartOfGeneralBalance,
                IsPartOfGeneralBalance_New = model.IsPartOfGeneralBalance,
            });

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.WalletRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.WalletRepository.GetByIdAsync(id);
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get wallet from database by id - {id.ToString()}");
            }

            unitOfWork.WalletRepository.HardDelete(domainModelToDelete);
            var walletDeletedEventModel = mapper.Map<WalletDeletedEvent>(domainModelToDelete);
            await eventPublisher.PublishAsync(walletDeletedEventModel);
            await unitOfWork.SaveAsync();
        }
    }
}
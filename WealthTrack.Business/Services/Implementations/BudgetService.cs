using AutoMapper;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class BudgetService(IUnitOfWork unitOfWork, IWalletService walletService, IMapper mapper) : IBudgetService
    {
        public async Task<Guid> CreateAsync(BudgetUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var domainModel = mapper.Map<Budget>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            domainModel.Status = BudgetStatus.Active;
            var createdEntityId = await unitOfWork.BudgetRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<BudgetDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModel = await unitOfWork.BudgetRepository.GetByIdAsync(id, include);
            var result = mapper.Map<BudgetDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<BudgetDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.BudgetRepository.GetAllAsync(include);
            var result = mapper.Map<List<BudgetDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, BudgetUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var originalModel = await unitOfWork.BudgetRepository.GetByIdAsync(id);
            if (originalModel == null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {id.ToString()}");
            }

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.BudgetRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.BudgetRepository.GetByIdAsync(id, $"{nameof(Budget.Wallets)}");
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get budget from database by id - {id.ToString()}");
            }

            while (domainModelToDelete.Wallets.Any())
            {
                await walletService.HardDeleteAsync(domainModelToDelete.Wallets.First().Id);
            }

            await unitOfWork.BudgetRepository.HardDeleteAsync(domainModelToDelete);
            await unitOfWork.SaveAsync();
        }
    }
}

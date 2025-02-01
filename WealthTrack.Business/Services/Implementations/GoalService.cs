using AutoMapper;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Services.Implementations
{
    public class GoalService(IUnitOfWork unitOfWork, IMapper mapper) : IGoalService
    {
        public async Task<Guid> CreateAsync(GoalUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var domainModel = mapper.Map<Goal>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, model.WalletIds, domainModel);
            var createdEntityId = await unitOfWork.GoalRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<GoalDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.GoalRepository.GetByIdAsync(id, include);
            var result = mapper.Map<GoalDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<GoalDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.GoalRepository.GetAllAsync(include);
            var result = mapper.Map<List<GoalDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, GoalUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var originalModel = await unitOfWork.GoalRepository.GetByIdAsync(id, "Categories,Wallets");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, model.WalletIds, originalModel);
            unitOfWork.GoalRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.GoalRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            await unitOfWork.SaveAsync();
            return true;
        }

        private async Task LoadRelatedEntitiesByIdsAsync(List<Guid>? categoryIds, List<Guid>? walletIds, Goal domainModel)
        {
            if (categoryIds != null)
            {
                foreach (var categoryId in categoryIds)
                {
                    var category = await unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
                    if (category != null)
                    {
                        domainModel.Categories.Add(category);
                    }
                }
            }

            if (walletIds != null)
            {
                foreach (var walletId in walletIds)
                {
                    var wallet = await unitOfWork.WalletRepository.GetByIdAsync(walletId);
                    if (wallet != null)
                    {
                        domainModel.Wallets.Add(wallet);
                    }
                }
            }
        }
    }
}

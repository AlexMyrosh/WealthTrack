using AutoMapper;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class GoalService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher) : IGoalService
    {
        public async Task<Guid> CreateAsync(GoalUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            if (model.PlannedMoneyAmount is null)
            {
                throw new ArgumentNullException(nameof(model.PlannedMoneyAmount));
            }

            if (model.PlannedMoneyAmount < 0)
            {
                throw new ArgumentException("PlannedMoneyAmount must be greater than or equal to 0");
            }

            if (model.StartDate is null)
            {
                throw new ArgumentNullException(nameof(model.StartDate));
            }
            
            if (model.EndDate is null)
            {
                throw new ArgumentNullException(nameof(model.EndDate));
            }
            
            if (model.Type is null)
            {
                throw new ArgumentNullException(nameof(model.Type));
            }
            
            if (!Enum.IsDefined(typeof(GoalType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
            }
            
            if (model.CategoryIds is null || !model.CategoryIds.Any())
            {
                throw new ArgumentNullException(nameof(model.CategoryIds));
            }

            if (model.EndDate < model.StartDate)
            {
                throw new ArgumentException("EndDate must be greater than or equal to StartDate");
            }
            
            var domainModel = mapper.Map<Goal>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, model.WalletIds, domainModel);
            if (!IsGoalHasCategoriesWithTheSameType(model.Type.Value, domainModel.Categories))
            {
                throw new ArgumentException("The type of selected categories should align with goal's type");
            }
            
            var createdEntityId = await unitOfWork.GoalRepository.CreateAsync(domainModel);
            var goalCreatedEventModel = mapper.Map<GoalCreatedEvent>(domainModel);
            await eventPublisher.PublishAsync(goalCreatedEventModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<GoalDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
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
                throw new ArgumentException(nameof(id));
            }
            
            if (model.Type.HasValue)
            {
                throw new InvalidOperationException("Goal type cannot be changed");
            }
            
            if (model.PlannedMoneyAmount < 0)
            {
                throw new ArgumentException("PlannedMoneyAmount must be greater than or equal to 0");
            }

            var originalModel = await unitOfWork.GoalRepository.GetByIdAsync(id, $"{nameof(Goal.Categories)}");
            if (originalModel is null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }
            
            mapper.Map(model, originalModel);
            
            if (originalModel.EndDate < originalModel.StartDate)
            {
                throw new ArgumentException("EndDate must be greater than or equal to StartDate");
            }
            
            originalModel.ModifiedDate = DateTimeOffset.Now;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, model.WalletIds, originalModel);
            if (!IsGoalHasCategoriesWithTheSameType(originalModel.Type, originalModel.Categories))
            {
                throw new ArgumentException("The type of selected categories should align with goal's type");
            }
            
            var goalCreatedEventModel = mapper.Map<GoalUpdatedEvent>(originalModel);
            await eventPublisher.PublishAsync(goalCreatedEventModel);
            unitOfWork.GoalRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.GoalRepository.GetByIdAsync(id);
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get goal from database by id - {id.ToString()}");
            }

            unitOfWork.GoalRepository.HardDelete(domainModelToDelete);

            await unitOfWork.SaveAsync();
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
        }

        private bool IsGoalHasCategoriesWithTheSameType(GoalType  goalType, List<Category> categories)
        {
            if (goalType == GoalType.Income)
            {
                return !categories.Exists(c => c.Type != CategoryType.Income);
            }
            
            if (goalType == GoalType.Expense)
            {
                return !categories.Exists(c => c.Type != CategoryType.Expense);
            }

            return false;
        }
    }
}

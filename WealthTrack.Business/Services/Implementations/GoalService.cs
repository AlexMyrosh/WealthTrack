using AutoMapper;
using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;
using WealthTrack.Shared.Extensions;

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
            
            if (!Enum.IsDefined(typeof(OperationType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
            }
            
            if (model.CategoryIds is null || !model.CategoryIds.Any())
            {
                throw new ArgumentNullException(nameof(model.CategoryIds));
            }

            if (model.EndDate <= model.StartDate)
            {
                throw new ArgumentException("EndDate must be greater than or equal to StartDate");
            }
            
            var domainModel = mapper.Map<Goal>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, domainModel);
            if (!IsGoalHasCategoriesWithTheSameType(model.Type.Value, domainModel.Categories))
            {
                throw new ArgumentException("The type of selected categories should align with goal's type");
            }
            
            var createdEntityId = await unitOfWork.GoalRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<GoalDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }
            
            var isIncludeUpdated = false;
            if (!include.Contains($"{nameof(Goal.Categories)}"))
            {
                isIncludeUpdated = true;
                if (string.IsNullOrWhiteSpace(include))
                {
                    include += $"{nameof(Goal.Categories)}";
                }
                else
                {
                    include += $",{nameof(Goal.Categories)}";
                }
            }

            var domainModel = await unitOfWork.GoalRepository.GetByIdAsync(id, include);
            var result = mapper.Map<GoalDetailsBusinessModel>(domainModel);
            if (result is not null)
            {
                var transactions = await unitOfWork.TransactionRepository.GetAllAsync(
                    filter: t => ((t.Type == TransactionType.Income && result.Type == OperationType.Income) ||
                                  (t.Type == TransactionType.Expense && result.Type == OperationType.Expense)) &&
                                 t.CategoryId.HasValue &&
                                 result.Categories.Select(c => c.Id).Contains(t.CategoryId.Value) &&
                                 t.TransactionDate >= result.StartDate && t.TransactionDate <= result.EndDate &&
                                 t.TransactionDate <= DateTimeOffset.UtcNow
                );
                result.ActualMoneyAmount = transactions.Sum(t => t.Amount);
                if (isIncludeUpdated)
                {
                    result.Categories = new List<CategoryRelatedToGoalDetailsBusinessModel>();
                }
            }
            
            return result;
        }

        public async Task<List<GoalDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var isIncludeUpdated = false;
            if (!include.Contains($"{nameof(Goal.Categories)}"))
            {
                isIncludeUpdated = true;
                if (string.IsNullOrWhiteSpace(include))
                {
                    include += $"{nameof(Goal.Categories)}";
                }
                else
                {
                    include += $",{nameof(Goal.Categories)}";
                }
            }
            
            var domainModels = await unitOfWork.GoalRepository.GetAllAsync(include);
            var result = mapper.Map<List<GoalDetailsBusinessModel>>(domainModels);
            foreach (var businessModel in result)
            {
                var transactions = await unitOfWork.TransactionRepository.GetAllAsync(
                    filter: t => t.Type == businessModel.Type.ToTransactionType() &&
                                 t.CategoryId.HasValue &&
                                 businessModel.Categories.Select(c => c.Id).Contains(t.CategoryId.Value) &&
                                 t.TransactionDate >= businessModel.StartDate &&
                                 t.TransactionDate <= businessModel.EndDate &&
                                 t.TransactionDate <= DateTimeOffset.UtcNow
                );
                businessModel.ActualMoneyAmount = transactions.Sum(t => t.Amount);
                if (isIncludeUpdated)
                {
                    businessModel.Categories = new List<CategoryRelatedToGoalDetailsBusinessModel>();
                }
            }
            
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
            
            if (originalModel.EndDate <= originalModel.StartDate)
            {
                throw new ArgumentException("EndDate must be greater than or equal to StartDate");
            }
            
            originalModel.ModifiedDate = DateTimeOffset.Now;
            await LoadRelatedEntitiesByIdsAsync(model.CategoryIds, originalModel);
            if (!originalModel.Categories.Any())
            {
                throw new ArgumentNullException(nameof(model.CategoryIds));
            }
            
            if (!IsGoalHasCategoriesWithTheSameType(originalModel.Type, originalModel.Categories))
            {
                throw new ArgumentException("The type of selected categories should align with goal's type");
            }
            
            unitOfWork.GoalRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id, bool shouldBeSaved = true)
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

            var domainModelsToDelete = await unitOfWork.GoalRepository.GetByIdsAsync(ids);
            if (domainModelsToDelete is null || domainModelsToDelete.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get goals from database by ids: {string.Join(", ", ids)}");
            }

            unitOfWork.GoalRepository.BulkHardDelete(domainModelsToDelete);
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }

        private async Task LoadRelatedEntitiesByIdsAsync(List<Guid>? categoryIds, Goal domainModel)
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
                    else
                    {
                        throw new ArgumentException($"Unable to find category with id - {categoryId.ToString()}");
                    }
                }
            }
        }

        private bool IsGoalHasCategoriesWithTheSameType(OperationType  goalType, List<Category> categories)
        {
            return !categories.Exists(c => c.Type != goalType);
        }
    }
}

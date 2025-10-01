using AutoMapper;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IEventPublisher eventPublisher) : ICategoryService
    {
        public async Task<Guid> CreateAsync(CategoryUpsertBusinessModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            if (!model.Type.HasValue)
            {
                throw new ArgumentNullException($"{nameof(model.Type)} should not be null.");
            }

            if (!Enum.IsDefined(typeof(OperationType), model.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(model.Type));
            }

            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = await unitOfWork.CategoryRepository.GetByIdAsync(model.ParentCategoryId.Value);
                if (parentCategory is null)
                {
                    throw new ArgumentException("Parent category not found.", nameof(model.ParentCategoryId));
                }

                if (parentCategory.Type != model.Type)
                {
                    throw new  ArgumentException("Parent category type not match.", nameof(model.Type));
                }
            }

            var domainModel = mapper.Map<Category>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = domainModel.CreatedDate;
            domainModel.Status = CategoryStatus.Active;
            var createdEntityId = await unitOfWork.CategoryRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            return createdEntityId;
        }

        public async Task<CategoryDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModel = await unitOfWork.CategoryRepository.GetByIdAsync(id, include);
            var result = mapper.Map<CategoryDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<CategoryDetailsBusinessModel>> GetAllAsync(string include = "")
        {
            var domainModels = await unitOfWork.CategoryRepository.GetAllAsync(include);
            var result = mapper.Map<List<CategoryDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, CategoryUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }
            
            if (model.Type.HasValue)
            {
                throw new InvalidOperationException("Category type cannot be changed");
            }

            var originalModel = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (originalModel == null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }
            
            if (model.ParentCategoryId.HasValue)
            {
                var parentCategory = await unitOfWork.CategoryRepository.GetByIdAsync(model.ParentCategoryId.Value);
                if (parentCategory is null)
                {
                    throw new ArgumentException("Parent category not found.", nameof(model.ParentCategoryId));
                }

                if (parentCategory.Type != originalModel.Type)
                {
                    throw new  ArgumentException("Parent category type not match.", nameof(model.Type));
                }
            }

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.CategoryRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id, bool shouldBeSaved = true)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModelToDelete = await unitOfWork.CategoryRepository.GetByIdAsync(id, $"{nameof(Category.ChildCategories)}");
            if (domainModelToDelete is null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }

            foreach (var childCategory in domainModelToDelete.ChildCategories)
            {
                await HardDeleteAsync(childCategory.Id, false);
            }

            unitOfWork.CategoryRepository.HardDelete(domainModelToDelete);
            var categoryDeletedEventModel = mapper.Map<CategoryDeletedEvent>(domainModelToDelete);
            await eventPublisher.PublishAsync(categoryDeletedEventModel);
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

            var domainModelsToDelete = await unitOfWork.CategoryRepository.GetByIdsAsync(ids, $"{nameof(Category.ChildCategories)}");
            if (domainModelsToDelete is null || domainModelsToDelete.Count == 0)
            {
                throw new KeyNotFoundException($"Unable to get categories from database by ids: {string.Join(", ", ids)}");
            }

            var childCategoryIdsToDelete = domainModelsToDelete.SelectMany(c => c.ChildCategories.Select(cc => cc.Id)).ToList();
            await BulkHardDeleteAsync(childCategoryIdsToDelete, false);
            unitOfWork.CategoryRepository.BulkHardDelete(domainModelsToDelete);
            foreach (var domainModelToDelete in domainModelsToDelete)
            {
                var categoryDeletedEventModel = mapper.Map<CategoryDeletedEvent>(domainModelToDelete);
                await eventPublisher.PublishAsync(categoryDeletedEventModel);
            }
            
            if (shouldBeSaved)
            {
                await unitOfWork.SaveAsync();
            }
        }
    }
}

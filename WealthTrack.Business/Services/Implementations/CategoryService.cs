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

            if (model.Type == CategoryType.System)
            {
                throw new UnauthorizedAccessException("You are not allowed to create this type of category.");
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
            domainModel.ModifiedDate = DateTimeOffset.Now;
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

        public async Task<List<CategoryDetailsBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.CategoryRepository.GetAllAsync();
            var result = mapper.Map<List<CategoryDetailsBusinessModel>>(domainModels);
            return result;
        }

        public async Task UpdateAsync(Guid id, CategoryUpsertBusinessModel model)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var originalModel = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (originalModel == null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }

            if (model.Type.HasValue && model.Type != originalModel.Type)
            {
                throw new InvalidOperationException("Category type cannot be changed");
            }

            mapper.Map(model, originalModel);
            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.CategoryRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task HardDeleteAsync(Guid id)
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
                unitOfWork.CategoryRepository.HardDelete(childCategory);
            }

            unitOfWork.CategoryRepository.HardDelete(domainModelToDelete);
            var categoryDeletedEventModel = mapper.Map<CategoryDeletedEvent>(domainModelToDelete);
            await eventPublisher.PublishAsync(categoryDeletedEventModel);
            await unitOfWork.SaveAsync();
        }
    }
}

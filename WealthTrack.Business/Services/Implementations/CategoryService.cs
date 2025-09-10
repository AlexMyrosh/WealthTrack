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

            var deletedDomainModel = await unitOfWork.CategoryRepository.GetByIdAsync(id, $"{nameof(Category.ChildCategories)}");
            if (deletedDomainModel is null)
            {
                throw new KeyNotFoundException($"Unable to get category from database by id - {id.ToString()}");
            }

            deletedDomainModel.ChildCategories.ForEach(cc => unitOfWork.CategoryRepository.HardDelete(cc));
            unitOfWork.CategoryRepository.HardDelete(deletedDomainModel);
            var categoryDeletedEventModel = mapper.Map<CategoryDeletedEvent>(deletedDomainModel);
            await eventPublisher.PublishAsync(categoryDeletedEventModel);
            await unitOfWork.SaveAsync();
        }
    }
}

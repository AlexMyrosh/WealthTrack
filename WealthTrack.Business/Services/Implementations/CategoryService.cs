using AutoMapper;
using WealthTrack.Business.BusinessModels.Category;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
    {
        public async Task CreateAsync(CreateCategoryBusinessModel model)
        {
            var domainModel = mapper.Map<Category>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            domainModel.Status = CategoryStatus.Active;
            await unitOfWork.CategoryRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<CategoryDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
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

        public async Task UpdateAsync(UpdateCategoryBusinessModel model)
        {
            var originalModel = await unitOfWork.CategoryRepository.GetByIdAsync(model.Id);
            mapper.Map(model, originalModel);
            if (originalModel is null)
            {
                throw new AutoMapperMappingException("Entity is null after mapping");
            }

            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.CategoryRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.CategoryRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            await unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.CategoryRepository.GetByIdAsync(id, "ChildCategories");
            if (domainModel is null)
            {
                return false;
            }

            // TODO: Add soft delete of all child categories (recursively). As an option, can Load method be used
            domainModel.Status = CategoryStatus.Deleted;
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}

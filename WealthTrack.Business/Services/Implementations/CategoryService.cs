using AutoMapper;
using WealthTrack.Business.BusinessModels;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
    {
        public async Task<CategoryBusinessModel> CreateAsync(CategoryBusinessModel model)
        {
            var domainModel = mapper.Map<Category>(model);
            var createdDomainModel = await unitOfWork.CategoryRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<CategoryBusinessModel>(createdDomainModel);
            return result;
        }

        public async Task<CategoryBusinessModel?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            var result = mapper.Map<CategoryBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<CategoryBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.CategoryRepository.GetAllAsync();
            var result = mapper.Map<List<CategoryBusinessModel>>(domainModels);
            return result;
        }

        public async Task<CategoryBusinessModel> UpdateAsync(CategoryBusinessModel model)
        {
            var domainModel = mapper.Map<Category>(model);
            var updatedDomainModel = unitOfWork.CategoryRepository.Update(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<CategoryBusinessModel>(updatedDomainModel);
            return result;
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

            var domainModel = await unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (domainModel is null)
            {
                return false;
            }

            domainModel.Status = CategoryStatus.Deleted;
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}

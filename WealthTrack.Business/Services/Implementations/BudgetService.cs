using AutoMapper;
using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class BudgetService(IUnitOfWork unitOfWork, IMapper mapper) : IBudgetService
    {
        public async Task CreateAsync(CreateBudgetBusinessModel model)
        {
            var domainModel = mapper.Map<Budget>(model);
            domainModel.CreatedDate = DateTimeOffset.Now;
            domainModel.ModifiedDate = DateTimeOffset.Now;
            domainModel.Status = BudgetStatus.Active;
            await unitOfWork.BudgetRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<BudgetDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "")
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
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

        public async Task UpdateAsync(UpdateBudgetBusinessModel model)
        {
            var originalModel = await unitOfWork.BudgetRepository.GetByIdAsync(model.Id);
            mapper.Map(model, originalModel);
            if (originalModel is null)
            {
                throw new AutoMapperMappingException("Entity is null after mapping");
            }

            originalModel.ModifiedDate = DateTimeOffset.Now;
            unitOfWork.BudgetRepository.Update(originalModel);
            await unitOfWork.SaveAsync();
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.BudgetRepository.HardDeleteAsync(id);
            if (deletedDomainModel is null)
            {
                return false;
            }

            await unitOfWork.SaveAsync();
            return true;
        }
    }
}

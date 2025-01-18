using AutoMapper;
using WealthTrack.Business.BusinessModels;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.UnitOfWork;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Services.Implementations
{
    public class CurrencyService(IUnitOfWork unitOfWork, IMapper mapper) : ICurrencyService
    {
        public async Task<CurrencyBusinessModel> CreateAsync(CurrencyBusinessModel model)
        {
            var domainModel = mapper.Map<Currency>(model);
            var createdDomainModel = await unitOfWork.CurrencyRepository.CreateAsync(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<CurrencyBusinessModel>(createdDomainModel);
            return result;
        }

        public async Task<CurrencyBusinessModel?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var domainModel = await unitOfWork.CurrencyRepository.GetByIdAsync(id);
            var result = mapper.Map<CurrencyBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<CurrencyBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.CurrencyRepository.GetAllAsync();
            var result = mapper.Map<List<CurrencyBusinessModel>>(domainModels);
            return result;
        }

        public async Task<CurrencyBusinessModel> UpdateAsync(CurrencyBusinessModel model)
        {
            var domainModel = mapper.Map<Currency>(model);
            var updatedDomainModel = unitOfWork.CurrencyRepository.Update(domainModel);
            await unitOfWork.SaveAsync();
            var result = mapper.Map<CurrencyBusinessModel>(updatedDomainModel);
            return result;
        }

        public async Task<bool> HardDeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id), "id is empty");
            }

            var deletedDomainModel = await unitOfWork.CurrencyRepository.HardDeleteAsync(id);
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

            var domainModel = await unitOfWork.CurrencyRepository.GetByIdAsync(id);
            if (domainModel is null)
            {
                return false;
            }

            domainModel.Status = CurrencyStatus.Deleted;
            await unitOfWork.SaveAsync();
            return true;
        }
    }
}
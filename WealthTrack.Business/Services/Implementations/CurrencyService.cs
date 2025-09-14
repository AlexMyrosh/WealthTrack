using AutoMapper;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Business.Services.Implementations
{
    public class CurrencyService(IUnitOfWork unitOfWork, IMapper mapper) : ICurrencyService
    {
        public async Task<CurrencyDetailsBusinessModel?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(nameof(id));
            }

            var domainModel = await unitOfWork.CurrencyRepository.GetByIdAsync(id);
            var result = mapper.Map<CurrencyDetailsBusinessModel>(domainModel);
            return result;
        }

        public async Task<List<CurrencyDetailsBusinessModel>> GetAllAsync()
        {
            var domainModels = await unitOfWork.CurrencyRepository.GetAllAsync();
            var result = mapper.Map<List<CurrencyDetailsBusinessModel>>(domainModels);
            return result;
        }
    }
}
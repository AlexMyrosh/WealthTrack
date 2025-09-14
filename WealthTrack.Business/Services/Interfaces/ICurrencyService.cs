using WealthTrack.Business.BusinessModels.Currency;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICurrencyService
    {
        public Task<CurrencyDetailsBusinessModel?> GetByIdAsync(Guid id);

        public Task<List<CurrencyDetailsBusinessModel>> GetAllAsync();
    }
}
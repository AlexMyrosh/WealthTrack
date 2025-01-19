using WealthTrack.Business.BusinessModels.Currency;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICurrencyService
    {
        public Task<CurrencyDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<CurrencyDetailsBusinessModel>> GetAllAsync(string include = "");
    }
}
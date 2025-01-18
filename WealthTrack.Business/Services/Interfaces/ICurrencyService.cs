using WealthTrack.Business.BusinessModels;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICurrencyService
    {
        public Task<CurrencyBusinessModel> CreateAsync(CurrencyBusinessModel model);

        public Task<CurrencyBusinessModel?> GetByIdAsync(Guid id);

        public Task<List<CurrencyBusinessModel>> GetAllAsync();

        public Task<CurrencyBusinessModel> UpdateAsync(CurrencyBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);

        public Task<bool> SoftDeleteAsync(Guid id);
    }
}
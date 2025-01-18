using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICurrencyRepository
    {
        public Task<Currency> CreateAsync(Currency model);

        public Task<Currency?> GetByIdAsync(Guid id);

        public Task<List<Currency>> GetAllAsync();

        public Currency Update(Currency model);

        public Task<Currency?> HardDeleteAsync(Guid id);

        public Currency HardDelete(Currency model);
    }
}
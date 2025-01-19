using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICurrencyRepository
    {
        public Task<Currency?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Currency>> GetAllAsync(string include = "");

        public Task<Currency> CreateAsync(Currency model);
    }
}
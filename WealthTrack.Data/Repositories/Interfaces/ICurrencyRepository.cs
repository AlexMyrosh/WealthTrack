using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICurrencyRepository
    {
        public Task<Currency?> GetByIdAsync(Guid id);

        public Task<List<Currency>> GetAllAsync();

        public Task<Guid> CreateAsync(Currency model);
    }
}
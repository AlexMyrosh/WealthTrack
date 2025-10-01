using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IBudgetRepository
    {
        public Task<Guid> CreateAsync(Budget model);

        public Task<Budget?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Budget>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "");

        public Task<List<Budget>> GetAllAsync(string include = "");

        public void Update(Budget model);

        public Task HardDeleteAsync(Budget budget);

        public void BulkHardDelete(IEnumerable<Budget> models);
    }
}
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IBudgetRepository
    {
        public Task<Budget> CreateAsync(Budget model);

        public Task<Budget?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Budget>> GetAllAsync(string include = "");

        public Budget Update(Budget model);

        public Task<Budget?> HardDeleteAsync(Guid id);

        public Budget HardDelete(Budget model);
    }
}
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<Guid> CreateAsync(Category model);

        public Task<Category?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Category>> GetAllAsync(string include = "");

        public Task<List<Category>> GetAllSystemOwnedAsync();

        public Category Update(Category model);

        public Task<Category?> HardDeleteAsync(Guid id);

        public Category HardDelete(Category model);
    }
}

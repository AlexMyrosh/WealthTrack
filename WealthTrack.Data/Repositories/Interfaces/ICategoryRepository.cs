using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<Guid> CreateAsync(Category model);

        public Task<Category?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Category>> GetAllAsync(string include = "");

        public Task<List<Category>> GetAllSystemOwnedAsync();

        public void Update(Category model);

        public void HardDelete(Category model);
    }
}

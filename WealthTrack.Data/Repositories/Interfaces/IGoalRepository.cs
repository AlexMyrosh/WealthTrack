using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IGoalRepository
    {
        public Task<Guid> CreateAsync(Goal model);

        public Task<Goal?> GetByIdAsync(Guid id, string include = "");
        
        public Task<List<Goal>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "");

        public Task<List<Goal>> GetAllAsync(string include = "");

        public void Update(Goal model);

        public void HardDelete(Goal model);

        public void BulkHardDelete(IEnumerable<Goal> models);
    }
}

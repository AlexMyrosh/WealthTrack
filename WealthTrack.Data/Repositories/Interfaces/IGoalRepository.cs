using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IGoalRepository
    {
        public Task<Guid> CreateAsync(Goal model);

        public Task<Goal?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Goal>> GetAllAsync(string include = "");

        public Goal Update(Goal model);

        public Task<Goal?> HardDeleteAsync(Guid id);

        public Goal HardDelete(Goal model);
    }
}

using WealthTrack.Business.BusinessModels.Goal;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IGoalService
    {
        public Task<Guid> CreateAsync(GoalUpsertBusinessModel model);

        public Task<GoalDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<GoalDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, GoalUpsertBusinessModel model);

        public Task HardDeleteAsync(Guid id);
    }
}

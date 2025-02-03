using WealthTrack.Business.BusinessModels.Budget;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IBudgetService
    {
        public Task<Guid> CreateAsync(BudgetUpsertBusinessModel model);

        public Task<BudgetDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<BudgetDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, BudgetUpsertBusinessModel model);

        public Task HardDeleteAsync(Guid id);
    }
}

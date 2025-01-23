using WealthTrack.Business.BusinessModels.Budget;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IBudgetService
    {
        public Task CreateAsync(CreateBudgetBusinessModel model);

        public Task<BudgetDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<BudgetDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(UpdateBudgetBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);
    }
}

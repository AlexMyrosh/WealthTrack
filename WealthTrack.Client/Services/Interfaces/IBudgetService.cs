using WealthTrack.Client.Models;

namespace WealthTrack.Client.Services.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllAsync();
}
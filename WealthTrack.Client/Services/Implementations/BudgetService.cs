using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Services.Implementations;

public class BudgetService : IBudgetService
{
    private readonly ApiClient _client;
    public BudgetService(ApiClient client) => _client = client;

    public async Task<IEnumerable<BudgetDto>> GetAllAsync()
    {
        return await _client.GetAsync<IEnumerable<BudgetDto>>("/api/Budget") ?? [];
    }
}
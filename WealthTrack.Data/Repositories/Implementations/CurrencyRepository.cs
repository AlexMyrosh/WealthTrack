using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class CurrencyRepository(AppDbContext context) : ICurrencyRepository
    {
        public async Task<Currency?> GetByIdAsync(Guid id)
        {
            var result = await context.Currencies.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Currency>> GetAllAsync()
        {
            var result = await context.Currencies.ToListAsync();
            return result;
        }

        public async Task<Guid> CreateAsync(Currency model)
        {
            var result = await context.Currencies.AddAsync(model);
            return result.Entity.Id;
        }
    }
}
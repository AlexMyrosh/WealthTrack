using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class CurrencyRepository(AppDbContext context) : ICurrencyRepository
    {
        public async Task<Currency> CreateAsync(Currency model)
        {
            var result = await context.Currencies.AddAsync(model);
            return result.Entity;
        }

        public async Task<Currency?> GetByIdAsync(Guid id)
        {
            var result = await context.Currencies.FindAsync(id);
            return result;
        }

        public async Task<List<Currency>> GetAllAsync()
        {
            var result = await context.Currencies.ToListAsync();
            return result;
        }

        public Currency Update(Currency model)
        {
            var result = context.Currencies.Update(model);
            return result.Entity;
        }

        public async Task<Currency?> HardDeleteAsync(Guid id)
        {
            var model = await context.Currencies.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.Currencies.Remove(model);
            return result.Entity;
        }

        public Currency HardDelete(Currency model)
        {
            var result = context.Currencies.Remove(model);
            return result.Entity;
        }
    }
}
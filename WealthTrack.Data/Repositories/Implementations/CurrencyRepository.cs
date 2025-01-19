using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class CurrencyRepository(AppDbContext context) : ICurrencyRepository
    {
        public async Task<Currency?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Currencies.AsQueryable();
            var includeProperties = include.Split(",");
            foreach (var property in includeProperties)
            {
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Currency>> GetAllAsync(string include = "")
        {
            var query = context.Currencies.AsSplitQuery();
            var includeProperties = include.Split(",");
            foreach (var property in includeProperties)
            {
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public async Task<Currency> CreateAsync(Currency model)
        {
            var result = await context.Currencies.AddAsync(model);
            return result.Entity;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class BudgetRepository(AppDbContext context) : IBudgetRepository
    {
        public async Task<Guid> CreateAsync(Budget model)
        {
            var result = await context.Budgets.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Budget?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Budgets.AsQueryable();
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

        public async Task<List<Budget>> GetAllAsync(string include = "")
        {
            var query = context.Budgets.AsQueryable();
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

        public void Update(Budget model)
        {
            context.Budgets.Update(model);
        }

        public void HardDelete(Budget model)
        {
            context.Budgets.Remove(model);
        }
    }
}

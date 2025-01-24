using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class GoalRepository(AppDbContext context) : IGoalRepository
    {
        public async Task<Guid> CreateAsync(Goal model)
        {
            var result = await context.Goals.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Goal?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Goals.AsQueryable();
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

        public async Task<List<Goal>> GetAllAsync(string include = "")
        {
            var query = context.Goals.AsQueryable();
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

        public Goal Update(Goal model)
        {
            var result = context.Goals.Update(model);
            return result.Entity;
        }

        public async Task<Goal?> HardDeleteAsync(Guid id)
        {
            var model = await context.Goals.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.Goals.Remove(model);
            return result.Entity;
        }

        public Goal HardDelete(Goal model)
        {
            var result = context.Goals.Remove(model);
            return result.Entity;
        }
    }
}

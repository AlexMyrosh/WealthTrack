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
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Goal>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "")
        {
            var query = context.Goals.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.Where(g => ids.Contains(g.Id)).ToListAsync();
            return result;
        }

        public async Task<List<Goal>> GetAllAsync(string include = "")
        {
            var query = context.Goals.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public void Update(Goal model)
        {
            context.Goals.Update(model);
        }

        public void HardDelete(Goal model)
        {
            context.Goals.Remove(model);
        }
        
        public void BulkHardDelete(IEnumerable<Goal> models)
        {
            context.Goals.RemoveRange(models);
        }
    }
}

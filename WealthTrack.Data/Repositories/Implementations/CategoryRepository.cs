using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        public async Task<Guid> CreateAsync(Category model)
        {
            var result = await context.Categories.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Category?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Categories.Where(c => !c.IsSystem).AsQueryable();
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

        public async Task<List<Category>> GetAllAsync()
        {
            var result = await context.Categories.Where(c => !c.IsSystem).ToListAsync();
            return result;
        }

        public async Task<List<Category>> GetAllSystemOwnedAsync()
        {
            var result = await context.Categories.Where(c => !c.IsSystem).ToListAsync();
            return result;
        }

        public void Update(Category model)
        {
            context.Categories.Update(model);
        }

        public void HardDelete(Category model)
        {
            context.Categories.Remove(model);
        }
    }
}

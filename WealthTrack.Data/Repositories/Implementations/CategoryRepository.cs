using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

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
            var query = context.Categories.AsQueryable();
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

        public async Task<List<Category>> GetAllAsync(string include = "")
        {
            var query = context.Categories.AsQueryable();
            var includeProperties = include.Split(",");
            foreach (var property in includeProperties)
            {
                if (string.IsNullOrWhiteSpace(property) || 
                    string.Equals(property, "parentcategory", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property, "childcategories", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public Category Update(Category model)
        {
            var result = context.Categories.Update(model);
            return result.Entity;
        }

        public async Task<Category?> HardDeleteAsync(Guid id)
        {
            var model = await context.Categories.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.Categories.Remove(model);
            return result.Entity;
        }

        public Category HardDelete(Category model)
        {
            var result = context.Categories.Remove(model);
            return result.Entity;
        }
    }
}

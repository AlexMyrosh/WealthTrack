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
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "")
        {
            var query = context.Categories.Where(c => !c.IsSystem).AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.Where(c => ids.Contains(c.Id)).ToListAsync();
            return result;
        }

        public async Task<List<Category>> GetAllAsync(string include = "")
        {
            var query = context.Categories.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            if (includeProperties.Any(p => typeof(Category).GetProperty(p) is null))
            {
                throw new ArgumentException("Unable to load dependencies. Please make sure that the include parameter is set correctly.");
            }
            
            if (!includeProperties.Contains(nameof(Category.ChildCategories)))
            {
                query = query.Where(c => !c.ParentCategoryId.HasValue);
            }

            var intermediateResult = await query.Where(c => !c.IsSystem).ToListAsync();
            var finalResult = intermediateResult.Where(c => c.ParentCategoryId is null).ToList();
            return finalResult;
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
        
        public void BulkHardDelete(IEnumerable<Category> models)
        {
            context.Categories.RemoveRange(models);
        }
    }
}

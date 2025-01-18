using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class CategoryRepository(AppDbContext context) : ICategoryRepository
    {
        public async Task<Category> CreateAsync(Category model)
        {
            var result = await context.Categories.AddAsync(model);
            return result.Entity;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            var result = await context.Categories.FindAsync(id);
            return result;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            var result = await context.Categories.ToListAsync();
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

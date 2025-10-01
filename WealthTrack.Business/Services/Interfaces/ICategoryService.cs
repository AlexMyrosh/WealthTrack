using WealthTrack.Business.BusinessModels.Category;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<Guid> CreateAsync(CategoryUpsertBusinessModel model);

        public Task<CategoryDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<CategoryDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, CategoryUpsertBusinessModel model);

        public Task HardDeleteAsync(Guid id, bool shouldBeSaved = true);
        
        public Task BulkHardDeleteAsync(List<Guid> ids, bool shouldBeSaved = true);
    }
}
using WealthTrack.Business.BusinessModels.Category;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task CreateAsync(CreateCategoryBusinessModel model);

        public Task<CategoryDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<CategoryDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(UpdateCategoryBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);

        public Task<bool> SoftDeleteAsync(Guid id);
    }
}
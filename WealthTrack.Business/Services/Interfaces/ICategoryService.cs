using WealthTrack.Business.BusinessModels;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<CategoryBusinessModel> CreateAsync(CategoryBusinessModel model);

        public Task<CategoryBusinessModel?> GetByIdAsync(Guid id);

        public Task<List<CategoryBusinessModel>> GetAllAsync();

        public Task<CategoryBusinessModel> UpdateAsync(CategoryBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);

        public Task<bool> SoftDeleteAsync(Guid id);
    }
}
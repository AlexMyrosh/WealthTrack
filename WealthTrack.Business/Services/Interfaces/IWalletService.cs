using WealthTrack.Business.BusinessModels;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IWalletService
    {
        public Task<WalletBusinessModel> CreateAsync(WalletBusinessModel model);

        public Task<WalletBusinessModel?> GetByIdAsync(Guid id);

        public Task<List<WalletBusinessModel>> GetAllAsync();

        public Task<WalletBusinessModel> UpdateAsync(WalletBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);

        public Task<bool> SoftDeleteAsync(Guid id);

        public Task<bool> ArchivedAsync(Guid id);
    }
}

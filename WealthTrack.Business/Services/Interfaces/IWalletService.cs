using WealthTrack.Business.BusinessModels.Wallet;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IWalletService
    {
        public Task<Guid> CreateAsync(WalletUpsertBusinessModel model);

        public Task<WalletDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<WalletDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, WalletUpsertBusinessModel model);

        public Task HardDeleteAsync(Guid id, bool shouldBeSaved = true);
        
        public Task BulkHardDeleteAsync(List<Guid> ids, bool shouldBeSaved = true);
    }
}

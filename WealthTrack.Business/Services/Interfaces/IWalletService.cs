using WealthTrack.Business.BusinessModels.Wallet;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface IWalletService
    {
        public Task CreateAsync(CreateWalletBusinessModel model);

        public Task<WalletDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<WalletDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(UpdateWalletBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);
    }
}

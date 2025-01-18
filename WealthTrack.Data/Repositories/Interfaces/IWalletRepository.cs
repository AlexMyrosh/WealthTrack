using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        public Task<Wallet> CreateAsync(Wallet model);

        public Task<Wallet?> GetByIdAsync(Guid id);

        public Task<List<Wallet>> GetAllAsync();

        public Wallet Update(Wallet model);

        public Task<Wallet?> HardDeleteAsync(Guid id);

        public Wallet HardDelete(Wallet model);
    }
}

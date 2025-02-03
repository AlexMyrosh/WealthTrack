using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        public Task<Guid> CreateAsync(Wallet model);

        public Task<Wallet?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Wallet>> GetAllAsync(string include = "");

        public void Update(Wallet model);

        public void HardDelete(Wallet model);
    }
}

using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ITransferTransactionRepository
    {
        public Task<Guid> CreateAsync(TransferTransaction model);

        public Task<TransferTransaction?> GetByIdAsync(Guid id, string include = "");

        public Task<List<TransferTransaction>> GetAllAsync(string include = "");

        public void Update(TransferTransaction model);

        public void HardDelete(TransferTransaction model);
    }
}

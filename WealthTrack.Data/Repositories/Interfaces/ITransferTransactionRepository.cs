using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ITransferTransactionRepository
    {
        public Task<Guid> CreateAsync(TransferTransaction model);

        public Task<TransferTransaction?> GetByIdAsync(Guid id, string include = "");

        public Task<List<TransferTransaction>> GetAllAsync(string include = "");

        public TransferTransaction Update(TransferTransaction model);

        public Task<TransferTransaction?> HardDeleteAsync(Guid id);

        public TransferTransaction HardDelete(TransferTransaction model);
    }
}

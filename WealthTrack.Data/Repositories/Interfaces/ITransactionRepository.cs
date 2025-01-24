using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        public Task<Guid> CreateAsync(Transaction model);

        public Task<Transaction?> GetByIdAsync(Guid id, string include = "");

        public Task<List<Transaction>> GetAllAsync(string include = "");

        public Transaction Update(Transaction model);

        public Task<Transaction?> HardDeleteAsync(Guid id);

        public Transaction HardDelete(Transaction model);
    }
}

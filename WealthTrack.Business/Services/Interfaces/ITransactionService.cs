using WealthTrack.Business.BusinessModels.Transaction;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task CreateAsync(TransactionUpsertBusinessModel model);

        public Task<TransactionDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, TransactionUpsertBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);
    }
}

using WealthTrack.Business.BusinessModels.Transaction;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<Guid> CreateAsync(TransactionUpsertBusinessModel model);

        public Task<Guid> CreateAsync(TransferTransactionUpsertBusinessModel model);

        public Task<TransactionDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(Guid id, TransactionUpsertBusinessModel model);

        public Task UpdateAsync(Guid id, TransferTransactionUpsertBusinessModel model);
        
        public Task UnassignCategoryAsync(Guid id);

        public Task HardDeleteAsync(Guid id, bool shouldBeSaved = true);
        
        public Task BulkHardDeleteAsync(List<Guid> ids, bool shouldBeSaved = true);
        
        public Task ArchiveAsync(Guid id, bool shouldBeSaved = true);
        
        public Task BulkArchiveAsync(List<Guid> ids, bool shouldBeSaved = true);
    }
}

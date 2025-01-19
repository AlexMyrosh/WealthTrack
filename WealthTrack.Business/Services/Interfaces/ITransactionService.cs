using WealthTrack.Business.BusinessModels;
using WealthTrack.Business.BusinessModels.Transaction;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task CreateAsync(CreateTransactionBusinessModel model);

        public Task<TransactionDetailsBusinessModel?> GetByIdAsync(Guid id, string include = "");

        public Task<List<TransactionDetailsBusinessModel>> GetAllAsync(string include = "");

        public Task UpdateAsync(UpdateTransactionBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);
    }
}

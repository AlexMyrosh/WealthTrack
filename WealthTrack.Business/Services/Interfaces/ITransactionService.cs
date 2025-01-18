using WealthTrack.Business.BusinessModels;

namespace WealthTrack.Business.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<TransactionBusinessModel> CreateAsync(TransactionBusinessModel model);

        public Task<TransactionBusinessModel?> GetByIdAsync(Guid id);

        public Task<List<TransactionBusinessModel>> GetAllAsync();

        public Task<TransactionBusinessModel> UpdateAsync(TransactionBusinessModel model);

        public Task<bool> HardDeleteAsync(Guid id);
    }
}

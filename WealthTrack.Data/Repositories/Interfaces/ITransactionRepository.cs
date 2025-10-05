using System.Linq.Expressions;
using WealthTrack.Data.DomainModels;

namespace WealthTrack.Data.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        public Task<Guid> CreateAsync(Transaction model);

        public Task<Transaction?> GetByIdAsync(Guid id, string include = "");
        
        public Task<List<Transaction>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "");

        public Task<List<Transaction>> GetAllAsync(string include = "", Expression<Func<Transaction, bool>>? filter = null);
        
        public Task<List<Transaction>> GetPageAsync(int pageNumber, int pageSize, string include = "");
        
        public Task<int> GetCountAsync();

        public void Update(Transaction model);

        public void HardDelete(Transaction model);

        public void BulkHardDelete(IEnumerable<Transaction> models);
    }
}

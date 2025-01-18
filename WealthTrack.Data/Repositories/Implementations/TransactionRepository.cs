using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class TransactionRepository(AppDbContext context) : ITransactionRepository
    {
        public async Task<Transaction> CreateAsync(Transaction model)
        {
            var result = await context.Transactions.AddAsync(model);
            return result.Entity;
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            var result = await context.Transactions.FindAsync(id);
            return result;
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            var result = await context.Transactions.ToListAsync();
            return result;
        }

        public Transaction Update(Transaction model)
        {
            var result = context.Transactions.Update(model);
            return result.Entity;
        }

        public async Task<Transaction?> HardDeleteAsync(Guid id)
        {
            var model = await context.Transactions.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.Transactions.Remove(model);
            return result.Entity;
        }

        public Transaction HardDelete(Transaction model)
        {
            var result = context.Transactions.Remove(model);
            return result.Entity;
        }
    }
}

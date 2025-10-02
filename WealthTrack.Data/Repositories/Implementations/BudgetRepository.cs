using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class BudgetRepository(AppDbContext context) : IBudgetRepository
    {
        public async Task<Guid> CreateAsync(Budget model)
        {
            var result = await context.Budgets.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Budget?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Budgets.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }
        
        public async Task<List<Budget>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "")
        {
            var query = context.Budgets.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.Where(b => ids.Contains(b.Id)).ToListAsync();
            return result;
        }

        public async Task<List<Budget>> GetAllAsync(string include = "")
        {
            var query = context.Budgets.Where(b => b.Status != EntityStatus.Archived).AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public void Update(Budget model)
        {
            context.Budgets.Update(model);
        }

        public async Task HardDeleteAsync(Budget budget)
        {
            // var walletIds = await context.Wallets
            //     .Where(w => w.BudgetId == budget.Id)
            //     .Select(w => w.Id)
            //     .ToListAsync();
            //
            // if (walletIds.Any())
            // {
            //     await context.Transactions
            //         .Where(t => walletIds.Contains(t.WalletId))
            //         .ExecuteDeleteAsync();
            //
            //     await context.TransferTransactions
            //         .Where(tt => walletIds.Contains(tt.SourceWalletId) || walletIds.Contains(tt.TargetWalletId))
            //         .ExecuteDeleteAsync();
            // }

            //context.Budgets.Remove(budget);
            context.Entry(budget).State = EntityState.Deleted;
            //await context.SaveChangesAsync();
        }

        public void BulkHardDelete(IEnumerable<Budget> models)
        {
            context.Budgets.RemoveRange(models);
        }
    }
}

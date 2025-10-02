using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class WalletRepository(AppDbContext context) : IWalletRepository
    {
        public async Task<Guid> CreateAsync(Wallet model)
        {
            var result = await context.Wallets.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Wallet?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Wallets.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Wallet>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "")
        {
            var query = context.Wallets.AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.Where(w => ids.Contains(w.Id)).ToListAsync();
            return result;
        }

        public async Task<List<Wallet>> GetAllAsync(string include = "")
        {
            var query = context.Wallets.Where(w => w.Status != EntityStatus.Archived).AsQueryable();
            var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public void Update(Wallet model)
        {
            context.Wallets.Update(model);
        }

        public async Task HardDeleteAsync(Wallet model)
        {
            // await context.TransferTransactions
            //     .Where(t => t.SourceWalletId == model.Id || t.TargetWalletId == model.Id)
            //     .ExecuteDeleteAsync();

            context.Entry(model).State = EntityState.Deleted;
            //context.Wallets.Remove(model);
        }
        
        public void BulkHardDelete(IEnumerable<Wallet> models)
        {
            context.Wallets.RemoveRange(models);
        }
    }
}

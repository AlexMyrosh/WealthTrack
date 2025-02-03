using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

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
            var includeProperties = include.Split(",");
            foreach (var property in includeProperties)
            {
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                query = EntityFrameworkQueryableExtensions.Include(query, property);
            }

            var result = await query.SingleOrDefaultAsync(e => e.Id == id);
            return result;
        }

        public async Task<List<Wallet>> GetAllAsync(string include = "")
        {
            var query = context.Wallets.AsQueryable();
            var includeProperties = include.Split(",");
            foreach (var property in includeProperties)
            {
                if (string.IsNullOrWhiteSpace(property))
                {
                    continue;
                }

                query = query.Include(property);
            }

            var result = await query.ToListAsync();
            return result;
        }

        public void Update(Wallet model)
        {
            context.Wallets.Update(model);
        }

        public void HardDelete(Wallet model)
        {
            context.Wallets.Remove(model);
        }
    }
}

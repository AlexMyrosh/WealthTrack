using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class WalletRepository(AppDbContext context) : IWalletRepository
    {
        public async Task<Wallet> CreateAsync(Wallet model)
        {
            var result = await context.Wallets.AddAsync(model);
            return result.Entity;
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
            var query = context.Wallets.AsSplitQuery();
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

        public Wallet Update(Wallet model)
        {
            var result = context.Wallets.Update(model);
            return result.Entity;
        }

        public async Task<Wallet?> HardDeleteAsync(Guid id)
        {
            var model = await context.Wallets.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.Wallets.Remove(model);
            return result.Entity;
        }

        public Wallet HardDelete(Wallet model)
        {
            var result = context.Wallets.Remove(model);
            return result.Entity;
        }
    }
}

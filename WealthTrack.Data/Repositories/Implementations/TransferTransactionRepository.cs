﻿using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class TransferTransactionRepository(AppDbContext context) : ITransferTransactionRepository
    {
        public async Task<Guid> CreateAsync(TransferTransaction model)
        {
            var result = await context.TransferTransactions.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<TransferTransaction?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.TransferTransactions.AsQueryable();
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

        public async Task<List<TransferTransaction>> GetAllAsync(string include = "")
        {
            var query = context.TransferTransactions.AsQueryable();
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

        public TransferTransaction Update(TransferTransaction model)
        {
            var result = context.TransferTransactions.Update(model);
            return result.Entity;
        }

        public async Task<TransferTransaction?> HardDeleteAsync(Guid id)
        {
            var model = await context.TransferTransactions.FindAsync(id);
            if (model is null)
            {
                return null;
            }

            var result = context.TransferTransactions.Remove(model);
            return result.Entity;
        }

        public TransferTransaction HardDelete(TransferTransaction model)
        {
            var result = context.TransferTransactions.Remove(model);
            return result.Entity;
        }
    }
}

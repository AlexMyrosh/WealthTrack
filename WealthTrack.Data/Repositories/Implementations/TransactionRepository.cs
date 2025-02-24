﻿using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.Repositories.Implementations
{
    public class TransactionRepository(AppDbContext context) : ITransactionRepository
    {
        public async Task<Guid> CreateAsync(Transaction model)
        {
            var result = await context.Transactions.AddAsync(model);
            return result.Entity.Id;
        }

        public async Task<Transaction?> GetByIdAsync(Guid id, string include = "")
        {
            var query = context.Transactions.AsQueryable();
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

        public async Task<List<Transaction>> GetAllAsync(string include = "")
        {
            var query = context.Transactions.AsQueryable();
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

        public void Update(Transaction model)
        {
            context.Transactions.Update(model);
        }

        public void HardDelete(Transaction model)
        {
            context.Transactions.Remove(model);
        }
    }
}

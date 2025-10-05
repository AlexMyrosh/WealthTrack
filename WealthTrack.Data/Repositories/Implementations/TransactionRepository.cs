using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WealthTrack.Data.Context;
using WealthTrack.Data.DomainModels;
using WealthTrack.Data.Repositories.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.Repositories.Implementations;

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
        var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
        foreach (var property in includeProperties)
        {
            query = EntityFrameworkQueryableExtensions.Include(query, property);
        }

        var result = await query.SingleOrDefaultAsync(e => e.Id == id);
        return result;
    }

    public async Task<List<Transaction>> GetByIdsAsync(IEnumerable<Guid> ids, string include = "")
    {
        var query = context.Transactions.AsQueryable();
        var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
        foreach (var property in includeProperties)
        {
            query = EntityFrameworkQueryableExtensions.Include(query, property);
        }

        var result = await query.Where(t => ids.Contains(t.Id)).ToListAsync();
        return result;
    }

    public async Task<List<Transaction>> GetAllAsync(string include = "", Expression<Func<Transaction, bool>>? filter = null)
    {
        var query = context.Transactions.Where(t => t.Status != EntityStatus.Archived).AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
        foreach (var property in includeProperties)
        {
            query = query.Include(property);
        }

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<List<Transaction>> GetPageAsync(int pageNumber, int pageSize, string include = "")
    {
        var query = context.Transactions.Where(t => t.Status != EntityStatus.Archived).AsQueryable();
        var includeProperties = string.IsNullOrWhiteSpace(include) ? [] : include.Split(",");
        foreach (var property in includeProperties)
        {
            query = query.Include(property);
        }
        
        query = query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var result = await query.ToListAsync();
        return result;
    }

    public async Task<int> GetCountAsync()
    {
        var result = await context.Transactions.CountAsync();
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

    public void BulkHardDelete(IEnumerable<Transaction> models)
    {
        context.Transactions.RemoveRange(models);
    }
}
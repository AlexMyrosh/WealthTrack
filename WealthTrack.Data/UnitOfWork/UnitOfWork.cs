using WealthTrack.Data.Context;
using WealthTrack.Data.Repositories.Implementations;
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(_context);
            TransactionRepository = new TransactionRepository(_context);
            CurrencyRepository = new CurrencyRepository(_context);
            WalletRepository = new WalletRepository(_context);
        }

        public ICategoryRepository CategoryRepository { get; }

        public ITransactionRepository TransactionRepository { get; }

        public ICurrencyRepository CurrencyRepository { get; }

        public IWalletRepository WalletRepository { get; }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
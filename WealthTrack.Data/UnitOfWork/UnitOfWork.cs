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
            TransferTransactionRepository = new TransferTransactionRepository(_context);
            CurrencyRepository = new CurrencyRepository(_context);
            WalletRepository = new WalletRepository(_context);
            BudgetRepository = new BudgetRepository(_context);
            GoalRepository = new GoalRepository(_context);
        }

        public ICategoryRepository CategoryRepository { get; }

        public ITransactionRepository TransactionRepository { get; }

        public ITransferTransactionRepository TransferTransactionRepository { get; }

        public ICurrencyRepository CurrencyRepository { get; }

        public IWalletRepository WalletRepository { get; }

        public IBudgetRepository BudgetRepository { get; }

        public IGoalRepository GoalRepository { get; }

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
using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public ICategoryRepository CategoryRepository { get; }

        public ITransactionRepository TransactionRepository { get; }

        public ITransferTransactionRepository TransferTransactionRepository { get; }

        public ICurrencyRepository CurrencyRepository { get; }

        public IWalletRepository WalletRepository { get; }

        public IBudgetRepository BudgetRepository { get; }

        public IGoalRepository GoalRepository { get; }

        Task<int> SaveAsync();
    }
}

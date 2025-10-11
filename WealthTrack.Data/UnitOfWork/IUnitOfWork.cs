using WealthTrack.Data.Repositories.Interfaces;

namespace WealthTrack.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public ICategoryRepository CategoryRepository { get; }

        public ITransactionRepository TransactionRepository { get; }

        public ICurrencyRepository CurrencyRepository { get; }

        public IWalletRepository WalletRepository { get; }

        public IGoalRepository GoalRepository { get; }

        Task<int> SaveAsync();
    }
}

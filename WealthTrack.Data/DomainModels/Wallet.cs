using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        // TODO: rename to IsPartOfBudgetBalance
        public bool IsPartOfGeneralBalance { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public EntityStatus Status { get; set; }

        public WalletType Type { get; set; }

        public Guid CurrencyId { get; set; }

        public Currency Currency { get; set; }

        public Guid BudgetId { get; set; }

        public Budget Budget { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<Transaction> IncomeTransferTransactions { get; set; }

        public List<Transaction> OutgoingTransferTransactions { get; set; }
    }
}

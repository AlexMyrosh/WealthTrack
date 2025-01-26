using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Budget
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal OverallBalance { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public BudgetStatus Status { get; set; }

        public Guid CurrencyId { get; set; }

        public Currency Currency { get; set; }

        public List<Wallet> Wallets { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }
        
        public bool IsPartOfGeneralBalance { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public EntityStatus Status { get; set; }
        
        public Guid CurrencyId { get; set; }

        public Currency Currency { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<Transaction> IncomeTransferTransactions { get; set; }

        public List<Transaction> OutgoingTransferTransactions { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class TransferTransaction
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        
        public DateTimeOffset ModifiedDate { get; set; }
        
        public EntityStatus Status { get; set; }

        public Guid SourceWalletId { get; set; }

        public Wallet SourceWallet { get; set; }

        public Guid TargetWalletId { get; set; }

        public Wallet TargetWallet { get; set; }
    }

}

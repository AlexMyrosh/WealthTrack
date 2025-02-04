namespace WealthTrack.Business.Events.Models
{
    public class TransferTransactionCreatedEvent
    {
        public decimal Amount { get; set; }

        public Guid SourceWalletId { get; set; }

        public Guid TargetWalletId { get; set; }
    }
}

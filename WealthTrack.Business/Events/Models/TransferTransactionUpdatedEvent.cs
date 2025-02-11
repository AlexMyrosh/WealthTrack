namespace WealthTrack.Business.Events.Models
{
    public class TransferTransactionUpdatedEvent
    {
        public decimal? Amount_New { get; set; }

        public decimal Amount_Old { get; set; }

        public Guid? SourceWalletId_New { get; set; }

        public Guid SourceWalletId_Old { get; set; }

        public Guid? TargetWalletId_New { get; set; }

        public Guid TargetWalletId_Old { get; set; }
    }
}

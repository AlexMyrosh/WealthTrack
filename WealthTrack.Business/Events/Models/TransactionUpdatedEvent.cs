using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionUpdatedEvent
    {
        public Guid? OldCategoryId { get; set; }

        public Guid? NewCategoryId { get; set; }

        public TransactionType OldTransactionType { get; set; }

        public TransactionType? NewTransactionType { get; set; }

        public Guid OldWalletId { get; set; }

        public Guid? NewWalletId { get; set; }

        public decimal OldAmount { get; set; }

        public decimal? NewAmount { get; set; }

        public DateTimeOffset OldTransactionDate { get; set; }

        public DateTimeOffset? NewTransactionDate { get; set; }
    }
}

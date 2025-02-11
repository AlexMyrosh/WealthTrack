using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionUpdatedEvent
    {
        public Guid? CategoryId_Old { get; set; }

        public Guid? CategoryId_New { get; set; }

        public TransactionType TransactionType_Old { get; set; }

        public TransactionType? TransactionType_New { get; set; }

        public Guid WalletId_Old { get; set; }

        public Guid? WalletId_New { get; set; }

        public decimal Amount_Old { get; set; }

        public decimal? Amount_New { get; set; }

        public DateTimeOffset TransactionDate_Old { get; set; }

        public DateTimeOffset? TransactionDate_New { get; set; }
    }
}

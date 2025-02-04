using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionDeletedEvent
    {
        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public Guid WalletId { get; set; }

        public Guid? CategoryId { get; set; }

        public DateTimeOffset TransactionDate { get; set; }
    }
}

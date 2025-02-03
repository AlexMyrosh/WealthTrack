using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionCreatedEvent
    {
        public TransactionType TransactionType { get; set; }

        public decimal Amount { get; set; }

        public Guid WalletId { get; set; }

        public Guid? CategoryId { get; set; }

        public DateTimeOffset TransactionDate { get; set; }
    }
}

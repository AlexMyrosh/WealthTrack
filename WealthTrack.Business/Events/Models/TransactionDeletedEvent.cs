using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionDeletedEvent(TransactionType transactionType, decimal amount, Guid walletId, Guid? categoryId, DateTimeOffset transactionDate)
    {
        public TransactionType TransactionType { get; } = transactionType;

        public decimal Amount { get; } = amount;

        public Guid WalletId { get; } = walletId;

        public Guid? CategoryId { get; } = categoryId;

        public DateTimeOffset TransactionDate { get; } = transactionDate;
    }
}

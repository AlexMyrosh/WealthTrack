using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class TransactionUpdatedEvent(Guid? oldCategoryId, Guid? newCategoryId, 
        TransactionType oldTransactionType, TransactionType? newTransactionType, 
        Guid oldWalletId, Guid? newWalletId, decimal oldAmount, decimal? newAmount,
        DateTimeOffset oldTransactionDate, DateTimeOffset? newTransactionDate)
    {
        public Guid? OldCategoryId { get; } = oldCategoryId;

        public Guid? NewCategoryId { get; } = newCategoryId;

        public TransactionType OldTransactionType { get; } = oldTransactionType;

        public TransactionType? NewTransactionType { get; } = newTransactionType;

        public Guid OldWalletId { get; } = oldWalletId;

        public Guid? NewWalletId { get; } = newWalletId;

        public decimal OldAmount { get; } = oldAmount;

        public decimal? NewAmount { get; } = newAmount;

        public DateTimeOffset OldTransactionDate { get; } = oldTransactionDate;

        public DateTimeOffset? NewTransactionDate { get; } = newTransactionDate;
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Transaction
{
    public class TransactionUpsertBusinessModel
    {
        public decimal? Amount { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset? TransactionDate { get; set; }

        public OperationType? Type { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? WalletId { get; set; }
    }
}

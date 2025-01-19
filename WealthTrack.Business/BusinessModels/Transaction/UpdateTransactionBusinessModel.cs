using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Transaction
{
    public class UpdateTransactionBusinessModel
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public TransactionType Type { get; set; }

        public Guid CategoryId { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels
{
    public class TransactionBusinessModel
    {
        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public TransactionType Type { get; set; }
    }
}
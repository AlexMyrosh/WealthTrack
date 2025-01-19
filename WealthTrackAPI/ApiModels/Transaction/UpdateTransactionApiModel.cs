using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Transaction
{
    public class UpdateTransactionApiModel
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public TransactionType Type { get; set; }

        public Guid CategoryId { get; set; }
    }
}

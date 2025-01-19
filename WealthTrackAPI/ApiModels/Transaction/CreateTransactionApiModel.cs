using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Transaction
{
    public class CreateTransactionApiModel
    {
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public TransactionType Type { get; set; }

        public Guid CategoryId { get; set; }

        public Guid WalletId { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Transaction
{
    public class TransactionDetailsApiModel
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public TransactionType Type { get; set; }

        public CategoryRelatedToTransactionDetailsApiModel Category { get; set; }

        public WalletRelatedToTransactionDetailsApiModel Wallet { get; set; }

        public WalletRelatedToTransactionDetailsApiModel SourceWallet { get; set; }

        public WalletRelatedToTransactionDetailsApiModel TargetWallet { get; set; }
    }

    public class CategoryRelatedToTransactionDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IconName { get; set; }
    }

    public class WalletRelatedToTransactionDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}

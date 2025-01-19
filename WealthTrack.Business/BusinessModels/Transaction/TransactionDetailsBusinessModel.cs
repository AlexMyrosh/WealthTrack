using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Transaction
{
    public class TransactionDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }

        public TransactionType Type { get; set; }

        public CategoryRelatedToTransactionDetailsBusinessModel Category { get; set; }

        public WalletRelatedToTransactionDetailsBusinessModel Wallet { get; set; }
    }

    public class CategoryRelatedToTransactionDetailsBusinessModel
    {
        public string Name { get; set; }

        public string IconName { get; set; }
    }

    public class WalletRelatedToTransactionDetailsBusinessModel
    {
        public string Name { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Wallet
{
    public class WalletDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }

        public CurrencyRelatedToWalletDetailsBusinessModel Currency { get; set; }

        public BudgetRelatedToWalletDetailsBusinessModel Budget { get; set; }
    }

    public class CurrencyRelatedToWalletDetailsBusinessModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }
    }

    public class BudgetRelatedToWalletDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}

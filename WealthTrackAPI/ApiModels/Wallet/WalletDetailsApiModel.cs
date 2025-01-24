using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Wallet
{
    public class WalletDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }

        public CurrencyRelatedToWalletDetailsApiModel Currency { get; set; }

        public BudgetRelatedToWalletDetailsApiModel Budget { get; set; }
    }

    public class CurrencyRelatedToWalletDetailsApiModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }
    }

    public class BudgetRelatedToWalletDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}

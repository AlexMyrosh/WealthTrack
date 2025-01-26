using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Budget
{
    public class BudgetDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal OverallBalance { get; set; }

        public CurrencyRelatedToBudgetDetailsApiModel Currency { get; set; }

        public List<WalletRelatedToBudgetDetailsApiModel> Wallets { get; set; }
    }

    public class CurrencyRelatedToBudgetDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }
    }

    public class WalletRelatedToBudgetDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }
    }
}
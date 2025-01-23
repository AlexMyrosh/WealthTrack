using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Budget
{
    public class BudgetDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public CurrencyRelatedToBudgetDetailsBusinessModel Currency { get; set; }

        public List<WalletRelatedToBudgetDetailsBusinessModel> Wallets { get; set; }
    }

    public class CurrencyRelatedToBudgetDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }
    }

    public class WalletRelatedToBudgetDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }
    }
}

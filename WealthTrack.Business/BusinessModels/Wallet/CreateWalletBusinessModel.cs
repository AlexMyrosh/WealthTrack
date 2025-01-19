using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Wallet
{
    public class CreateWalletBusinessModel
    {
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletType Type { get; set; }

        public Guid CurrencyId { get; set; }
    }
}

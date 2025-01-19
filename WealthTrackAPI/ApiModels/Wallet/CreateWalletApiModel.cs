using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Wallet
{
    public class CreateWalletApiModel
    {
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public WalletType Type { get; set; }

        public Guid CurrencyId { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels
{
    public class WalletBusinessModel
    {
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public WalletStatus Status { get; set; }

        public WalletType Type { get; set; }

        public CurrencyBusinessModel Currency { get; set; }

        public List<TransactionBusinessModel> Transactions { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Currency
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public CurrencyStatus Status { get; set; }

        public List<Wallet> Wallets { get; set; }
    }
}
using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Currency
{
    public class CurrencyDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }
        
        public CurrencyType Type { get; set; }
    }
}

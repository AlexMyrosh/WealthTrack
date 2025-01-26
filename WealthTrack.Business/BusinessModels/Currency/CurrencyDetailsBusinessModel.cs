namespace WealthTrack.Business.BusinessModels.Currency
{
    public class CurrencyDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal ExchangeRate { get; set; }
    }
}

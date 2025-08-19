using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestCurrencyModels
    {
        public static readonly Guid FirstCurrencyId = new("069f65d4-d953-4340-961b-0b96bc09570e");
        public static readonly Guid SecondCurrencyId = new("5aa87382-817c-4a1d-a233-7952fde4f18c");

        public static Currency FirstDomainModelWithoutDetails
        {
            get
            {
                return new Currency
                {
                    Id = FirstCurrencyId,
                    Code = "C1",
                    Name = "First",
                    Symbol = "S1",
                    ExchangeRate = 0.5M,
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat,
                };
            }
        }

        public static Currency SecondDomainModelWithoutDetails
        {
            get
            {
                return new Currency
                {
                    Id = SecondCurrencyId,
                    Code = "C2",
                    Name = "Second",
                    Symbol = "S2",
                    ExchangeRate = 1.5M,
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Crypto,
                };
            }
        }
    }
}

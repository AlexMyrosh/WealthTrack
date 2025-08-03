using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestCurrencyModels
    {
        public static readonly Guid CurrencyId = new("069f65d4-d953-4340-961b-0b96bc09570e");
        public static Currency DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Wallets = [TestWalletModels.DomainModelWithoutDetails];
                model.Budgets = [TestBudgetModels.DomainModelWithoutDetails];
                return model;
            }
        }

        public static CurrencyDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new CurrencyDetailsBusinessModel
                {
                    Id = CurrencyId,
                    Code = DomainModel.Code,
                    Name = DomainModel.Name,
                    Symbol = DomainModel.Symbol,
                    ExchangeRate = DomainModel.ExchangeRate
                };

                return model;
            }
        }

        public static Currency DomainModelWithoutDetails
        {
            get
            {
                return new Currency
                {
                    Id = CurrencyId,
                    Code = "Code",
                    Name = "Name",
                    Symbol = "Symbol",
                    ExchangeRate = 1.123M,
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat,
                };
            }
        }
    }
}

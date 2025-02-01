using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.TestModels
{
    public static class TestCurrencyModels
    {
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
                    Id = DomainModel.Id,
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
                    Id = Guid.NewGuid(),
                    Code = "Test currency code",
                    Name = "Test currency name",
                    Symbol = "Test currency symbol",
                    ExchangeRate = 1.123M,
                    Status = CurrencyStatus.Active,
                    Type = CurrencyType.Fiat,
                };
            }
        }
    }
}

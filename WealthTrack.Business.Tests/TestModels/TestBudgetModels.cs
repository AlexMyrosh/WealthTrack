using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.TestModels
{
    public static class TestBudgetModels
    {
        public static Budget DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Currency = TestCurrencyModels.DomainModelWithoutDetails;
                model.CurrencyId = model.Currency.Id;
                model.Wallets = [TestWalletModels.DomainModelWithoutDetails];
                return model;
            }
        }

        public static BudgetUpsertBusinessModel UpsertBusinessModel
        {
            get
            {
                var model = new BudgetUpsertBusinessModel
                {
                    Name = DomainModel.Name,
                    CurrencyId = DomainModel.Id
                };

                return model;
            }
        }

        public static BudgetDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new BudgetDetailsBusinessModel
                {
                    Id = DomainModel.Id,
                    Name = DomainModel.Name,
                    OverallBalance = DomainModel.OverallBalance,
                    Currency = new()
                    {
                        Id = TestCurrencyModels.DomainModel.Id,
                        Code = TestCurrencyModels.DomainModel.Code,
                        Name = TestCurrencyModels.DomainModel.Name,
                        Symbol = TestCurrencyModels.DomainModel.Symbol
                    },
                    Wallets =
                    [
                        new()
                        {
                            Id = TestWalletModels.DomainModel.Id,
                            Name = TestWalletModels.DomainModel.Name,
                            Balance = TestWalletModels.DomainModel.Balance,
                            IsPartOfGeneralBalance = TestWalletModels.DomainModel.IsPartOfGeneralBalance,
                            Status = TestWalletModels.DomainModel.Status,
                            Type = TestWalletModels.DomainModel.Type
                        }
                    ]
                };

                return model;
            }
        }

        public static Budget DomainModelWithoutDetails
        {
            get
            {
                return new Budget
                {
                    Id = Guid.NewGuid(),
                    Name = "Test budget domain model",
                    OverallBalance = 100.123M,
                    CreatedDate = DateTimeOffset.Now,
                    ModifiedDate = DateTimeOffset.Now,
                    Status = BudgetStatus.Active,
                };
            }
        }
    }
}

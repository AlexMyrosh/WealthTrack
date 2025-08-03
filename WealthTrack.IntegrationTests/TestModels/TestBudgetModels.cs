using WealthTrack.Business.BusinessModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestBudgetModels
    {
        public static readonly Guid BudgetId = new("f75a174e-9b43-49be-876c-879683c496fa");

        public static Budget DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Currency = TestCurrencyModels.DomainModelWithoutDetails;
                model.CurrencyId = TestCurrencyModels.CurrencyId;
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
                    CurrencyId = TestCurrencyModels.CurrencyId
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
                    Id = BudgetId,
                    Name = DomainModel.Name,
                    OverallBalance = DomainModel.OverallBalance,
                    Currency = new()
                    {
                        Id = TestCurrencyModels.CurrencyId,
                        Code = TestCurrencyModels.DomainModel.Code,
                        Name = TestCurrencyModels.DomainModel.Name,
                        Symbol = TestCurrencyModels.DomainModel.Symbol
                    },
                    Wallets =
                    [
                        new()
                        {
                            Id = TestWalletModels.WalletId,
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
                    Id = BudgetId,
                    Name = "Test budget domain model",
                    OverallBalance = 0M,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = BudgetStatus.Active,
                };
            }
        }
    }
}

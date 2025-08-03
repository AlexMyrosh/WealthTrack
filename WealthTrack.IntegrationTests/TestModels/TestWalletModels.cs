using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestWalletModels
    {
        public static readonly Guid WalletId = new("d8f37d0f-12f3-4998-bf1a-7b563a6baa3a");

        public static Wallet DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Currency = TestCurrencyModels.DomainModelWithoutDetails;
                model.CurrencyId = model.Currency.Id;
                model.Budget = TestBudgetModels.DomainModelWithoutDetails;
                model.BudgetId = model.Budget.Id;
                model.Transactions = [TestTransactionModels.DomainModelWithoutDetails];
                model.IncomeTransferTransactions = [TestTransactionModels.DomainModelWithoutDetails];
                model.OutgoingTransferTransactions = [TestTransactionModels.DomainModelWithoutDetails];
                return model;
            }
        }

        public static WalletUpsertBusinessModel UpsertBusinessModel
        {
            get
            {
                var model = new WalletUpsertBusinessModel
                {
                    Name = DomainModel.Name,
                    Balance = DomainModel.Balance,
                    IsPartOfGeneralBalance = DomainModel.IsPartOfGeneralBalance,
                    Type = DomainModel.Type,
                    CurrencyId = DomainModel.CurrencyId,
                    BudgetId = DomainModel.BudgetId
                };

                return model;
            }
        }

        public static WalletDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new WalletDetailsBusinessModel
                {
                    Id = DomainModel.Id,
                    Name = DomainModel.Name,
                    Balance = DomainModel.Balance,
                    IsPartOfGeneralBalance = DomainModel.IsPartOfGeneralBalance,
                    Status = DomainModel.Status,
                    Type = DomainModel.Type,
                    Currency = new CurrencyRelatedToWalletDetailsBusinessModel
                    {
                        Id = TestCurrencyModels.DomainModel.Id,
                        Code = TestCurrencyModels.DomainModel.Code,
                        Name = TestCurrencyModels.DomainModel.Name,
                        Symbol = TestCurrencyModels.DomainModel.Symbol,
                        ExchangeRate = TestCurrencyModels.DomainModel.ExchangeRate
                    },
                    Budget = new BudgetRelatedToWalletDetailsBusinessModel
                    {
                        Id = TestBudgetModels.DomainModel.Id,
                        Name = TestBudgetModels.DomainModel.Name,
                        OverallBalance = TestBudgetModels.DomainModel.OverallBalance
                    },
                    Transactions =
                    [
                        new()
                        {
                            Id = TestTransactionModels.TransactionDomainModel.Id,
                            Amount = TestTransactionModels.TransactionDomainModel.Amount,
                            Description = TestTransactionModels.TransactionDomainModel.Description,
                            TransactionDate = TestTransactionModels.TransactionDomainModel.TransactionDate,
                            Type = TestTransactionModels.TransactionDomainModel.Type
                        }
                    ]
                };

                return model;
            }
        }

        public static Wallet DomainModelWithoutDetails
        {
            get
            {
                return new Wallet
                {
                    Id = WalletId,
                    Name = "Test wallet name",
                    Balance = 200.123M,
                    IsPartOfGeneralBalance = true,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = WalletStatus.Active,
                    Type = WalletType.Cash,
                };
            }
        }
    }
}

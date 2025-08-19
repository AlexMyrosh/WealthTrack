using WealthTrack.API.ApiModels.Budget;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestBudgetModels
    {
        public static readonly Guid FirstBudgetId = new("f75a174e-9b43-49be-876c-879683c496fa");
        public static readonly Guid SecondBudgetId = new("dea06058-438f-4c62-8f22-575dbefaba90");

        public static BudgetUpsertApiModel UpsertApiModel
        {
            get
            {
                var model = new BudgetUpsertApiModel
                {
                    Name = Guid.NewGuid().ToString()
                };

                return model;
            }
        }

        public static Budget FirstDomainModelWithoutDetails
        {
            get
            {
                return new Budget
                {
                    Id = FirstBudgetId,
                    Name = "First Test Budget Domain Model",
                    OverallBalance = 0M,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = BudgetStatus.Active
                };
            }
        }

        public static Budget SecondDomainModelWithoutDetails
        {
            get
            {
                return new Budget
                {
                    Id = SecondBudgetId,
                    Name = "Second Test Budget Domain Model",
                    OverallBalance = 0M,
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    Status = BudgetStatus.Active
                };
            }
        }
    }
}

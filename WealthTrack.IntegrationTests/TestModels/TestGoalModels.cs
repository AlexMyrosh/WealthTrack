using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestGoalModels
    {
        public static readonly Guid GoalId = new("f8f79740-a3f0-4434-b6da-3dcce9788bc5");

        public static Goal DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Categories = [TestCategoryModels.DomainModelWithoutDetails];
                return model;
            }
        }

        public static GoalUpsertBusinessModel UpsertBusinessModel
        {
            get
            {
                var model = new GoalUpsertBusinessModel
                {
                    Name = DomainModel.Name,
                    PlannedMoneyAmount = DomainModel.PlannedMoneyAmount,
                    Type = DomainModel.Type,
                    StartDate = DomainModel.StartDate,
                    EndDate = DomainModel.EndDate,
                    CategoryIds = DomainModel.Categories.Select(x => x.Id).ToList()
                };

                return model;
            }
        }

        public static GoalDetailsBusinessModel DetailsBusinessModel
        {
            get
            {
                var model = new GoalDetailsBusinessModel
                {
                    Id = GoalId,
                    Name = DomainModel.Name,
                    PlannedMoneyAmount = DomainModel.PlannedMoneyAmount,
                    ActualMoneyAmount = DomainModel.ActualMoneyAmount,
                    Type = DomainModel.Type,
                    StartDate = DomainModel.StartDate,
                    EndDate = DomainModel.EndDate,
                    Categories =
                    [
                        new()
                        {
                            Id = TestCategoryModels.CategoryId,
                            Name = TestCategoryModels.DomainModel.Name,
                            IconName = TestCategoryModels.DomainModel.IconName,
                            Type = TestCategoryModels.DomainModel.Type
                        }
                    ],
                    Wallets =
                    [
                        new()
                        {
                            Id = TestWalletModels.WalletId,
                            Name = TestWalletModels.DomainModel.Name
                        }
                    ]
                };

                return model;
            }
        }

        public static Goal DomainModelWithoutDetails
        {
            get
            {
                return new Goal
                {
                    Id = GoalId,
                    Name = "Test goal domain model",
                    PlannedMoneyAmount = 400.123M,
                    ActualMoneyAmount = 300.123M,
                    Type = GoalType.Income,
                    StartDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    EndDate = new DateTimeOffset(2025, 1, 31, 12, 0, 0, TimeSpan.Zero),
                    CreatedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    ModifiedDate = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero)
                };
            }
        }
    }
}

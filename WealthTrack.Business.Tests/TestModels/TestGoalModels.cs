﻿using WealthTrack.Business.BusinessModels.Goal;
using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Tests.TestModels
{
    public static class TestGoalModels
    {
        public static Goal DomainModel
        {
            get
            {
                var model = DomainModelWithoutDetails;
                model.Categories = [TestCategoryModels.DomainModelWithoutDetails];
                model.Wallets = [TestWalletModels.DomainModelWithoutDetails];
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
                    CategoryIds = DomainModel.Categories.Select(x => x.Id).ToList(),
                    WalletIds = DomainModel.Wallets.Select(x => x.Id).ToList()
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
                    Id = DomainModel.Id,
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
                            Id = TestCategoryModels.DomainModel.Id,
                            Name = TestCategoryModels.DomainModel.Name,
                            IconName = TestCategoryModels.DomainModel.IconName,
                            Type = TestCategoryModels.DomainModel.Type
                        }
                    ],
                    Wallets =
                    [
                        new()
                        {
                            Id = TestWalletModels.DomainModel.Id,
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
                    Id = Guid.NewGuid(),
                    Name = "Test goal domain model",
                    PlannedMoneyAmount = 400.123M,
                    ActualMoneyAmount = 300.123M,
                    Type = GoalType.Income,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 1, 31),
                    CreatedDate = DateTimeOffset.Now,
                    ModifiedDate = DateTimeOffset.Now
                };
            }
        }
    }
}

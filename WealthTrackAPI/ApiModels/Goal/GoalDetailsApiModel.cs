using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Goal
{
    public class GoalDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal PlannedMoneyAmount { get; set; }

        public decimal ActualMoneyAmount { get; set; }

        public GoalType Type { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public List<CategoryRelatedToGoalDetailsApiModel> Categories { get; set; }

        public List<WalletRelatedToGoalDetailsApiModel> Wallets { get; set; }
    }

    public class CategoryRelatedToGoalDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public CategoryType Type { get; set; }
    }

    public class WalletRelatedToGoalDetailsApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}

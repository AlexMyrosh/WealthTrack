using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Goal
{
    public class GoalDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal PlannedMoneyAmount { get; set; }

        public decimal ActualMoneyAmount { get; set; }

        public OperationType Type { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public List<CategoryRelatedToGoalDetailsBusinessModel> Categories { get; set; }

        public List<WalletRelatedToGoalDetailsBusinessModel> Wallets { get; set; }
    }

    public class CategoryRelatedToGoalDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? IconName { get; set; }

        public OperationType Type { get; set; }
    }

    public class WalletRelatedToGoalDetailsBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}

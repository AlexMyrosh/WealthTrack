using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.BusinessModels.Goal
{
    public class GoalUpsertBusinessModel
    {
        public string? Name { get; set; }

        public decimal? PlannedMoneyAmount { get; set; }

        public OperationType? Type { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public List<Guid>? CategoryIds { get; set; }

        public List<Guid>? WalletIds { get; set; }
    }
}

using WealthTrack.Shared.Enums;

namespace WealthTrack.API.ApiModels.Goal
{
    public class GoalUpsertApiModel
    {
        public string? Name { get; set; }

        public decimal? PlannedMoneyAmount { get; set; }

        public OperationType? Type { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public List<Guid>? CategoryIds { get; set; }
    }
}

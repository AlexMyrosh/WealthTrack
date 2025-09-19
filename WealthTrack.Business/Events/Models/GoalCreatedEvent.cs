using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Business.Events.Models
{
    public class GoalCreatedEvent
    {
        public Goal  GoalModel { get; set; }
        public OperationType? Type { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public List<Guid> CategoryIds { get; set; }
    }
}

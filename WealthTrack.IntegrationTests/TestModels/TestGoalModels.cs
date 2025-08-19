using WealthTrack.Data.DomainModels;
using WealthTrack.Shared.Enums;

namespace WealthTrack.IntegrationTests.Helpers
{
    public static class TestGoalModels
    {
        public static readonly Guid GoalId = new("f8f79740-a3f0-4434-b6da-3dcce9788bc5");

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

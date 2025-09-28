using WealthTrack.Shared.Enums;

namespace WealthTrack.Data.DomainModels
{
    public class Goal
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal PlannedMoneyAmount { get; set; }

        public decimal ActualMoneyAmount { get; set; }

        public OperationType Type { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        public List<Category> Categories { get; set; }
    }
}
namespace WealthTrack.Business.Events.Models
{
    public class WalletUpdatedEvent
    {
        public Guid WalletId { get; set; }

        public Guid BudgetId_Old { get; set; }

        public Guid? BudgetId_New { get; set; }

        public decimal Balance_Old { get; set; }

        public decimal? Balance_New { get; set; }

        public bool IsPartOfGeneralBalance_Old { get; set; }

        public bool? IsPartOfGeneralBalance_New { get; set; }
    }
}

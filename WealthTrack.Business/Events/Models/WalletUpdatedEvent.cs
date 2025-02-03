namespace WealthTrack.Business.Events.Models
{
    public class WalletUpdatedEvent
    {
        public Guid WalletId { get; set; }

        public Guid OldBudgetId { get; set; }

        public Guid? NewBudgetId { get; set; }

        public decimal OldBalance { get; set; }

        public decimal? NewBalance { get; set; }

        public bool IsPartOfGeneralBalanceOldValue { get; set; }

        public bool? IsPartOfGeneralBalanceNewValue { get; set; }
    }
}

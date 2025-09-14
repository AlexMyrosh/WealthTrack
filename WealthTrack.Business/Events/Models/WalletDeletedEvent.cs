namespace WealthTrack.Business.Events.Models
{
    public class WalletDeletedEvent
    {
        public Guid WalletId { get; set; }
        public decimal Balance { get; set; }

        public bool IsPartOfGeneralBalance { get; set; }

        public Guid BudgetId { get; set; }
    }
}

namespace WealthTrack.Business.Events.Models
{
    public class WalletBalanceChangedEvent(Guid walletId, Guid budgetIdOld, Guid? budgetIdNew, decimal balanceOld, decimal? balanceNew, bool isPartOfGeneralBalanceOld, bool? isPartOfGeneralBalanceNew)
    {
        // TODO: create a generic model for creating new and old values
        public Guid WalletId { get; } = walletId;

        public Guid OldBudgetId { get; } = budgetIdOld;

        public Guid? NewBudgetId { get; } = budgetIdNew;

        public decimal OldBalance { get; } = balanceOld;

        public decimal? NewBalance { get; } = balanceNew;

        public bool IsPartOfGeneralBalanceOldValue { get; } = isPartOfGeneralBalanceOld;

        public bool? IsPartOfGeneralBalanceNewValue { get; } = isPartOfGeneralBalanceNew;
    }
}

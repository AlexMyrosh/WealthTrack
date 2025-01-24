namespace WealthTrack.Business.BusinessModels.Budget
{
    public class BudgetUpsertBusinessModel
    {
        public string Name { get; set; }

        public Guid CurrencyId { get; set; }
    }
}

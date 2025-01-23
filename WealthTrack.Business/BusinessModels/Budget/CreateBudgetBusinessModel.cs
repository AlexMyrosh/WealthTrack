namespace WealthTrack.Business.BusinessModels.Budget
{
    public class CreateBudgetBusinessModel
    {
        public string Name { get; set; }

        public Guid CurrencyId { get; set; }
    }
}

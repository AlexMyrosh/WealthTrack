namespace WealthTrack.Business.BusinessModels.Budget
{
    public class UpdateBudgetBusinessModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid CurrencyId { get; set; }
    }
}

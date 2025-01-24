namespace WealthTrack.API.ApiModels.Budget
{
    public class BudgetUpsertApiModel
    {
        public string? Name { get; set; }

        public Guid? CurrencyId { get; set; }
    }
}
namespace WealthTrack.API.ApiModels.Budget
{
    public class UpdateBudgetApiModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid CurrencyId { get; set; }
    }
}
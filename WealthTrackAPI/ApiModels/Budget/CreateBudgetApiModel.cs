﻿namespace WealthTrack.API.ApiModels.Budget
{
    public class CreateBudgetApiModel
    {
        public string Name { get; set; }

        public Guid CurrencyId { get; set; }
    }
}
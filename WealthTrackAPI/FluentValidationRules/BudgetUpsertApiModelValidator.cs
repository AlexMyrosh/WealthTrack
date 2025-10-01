using FluentValidation;
using WealthTrack.API.ApiModels.Budget;

namespace WealthTrack.API.FluentValidationRules;

public class BudgetUpsertApiModelValidator : AbstractValidator<BudgetUpsertApiModel>
{
    public BudgetUpsertApiModelValidator()
    {

    }
}
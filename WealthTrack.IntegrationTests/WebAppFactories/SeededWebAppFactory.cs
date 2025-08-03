using Microsoft.Extensions.DependencyInjection;
using WealthTrack.Business.Seeders;
using WealthTrack.Data.Context;

public class SeededWebAppFactory : BaseTestWebAppFactory
{
    public override async Task InitializeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var currencySeeder = scope.ServiceProvider.GetRequiredService<CurrenciesSeeder>();
        await currencySeeder.SeedAsync();

        var categorySeeder = scope.ServiceProvider.GetRequiredService<SystemCategoriesSeeder>();
        await categorySeeder.SeedAsync();
    }
}

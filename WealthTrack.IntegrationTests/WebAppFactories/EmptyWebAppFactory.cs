using Microsoft.Extensions.DependencyInjection;
using WealthTrack.Data.Context;
using WealthTrack.IntegrationTests.WebAppFactories.Base;

namespace WealthTrack.IntegrationTests.WebAppFactories;

public class EmptyWebAppFactory : BaseTestWebAppFactory
{
    public override async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
using Microsoft.Extensions.DependencyInjection;
using WealthTrack.Data.Context;
using WealthTrack.IntegrationTests.WebAppFactories.Base;

namespace WealthTrack.IntegrationTests.TestData;

public abstract class IntegrationTestBase(BaseTestWebAppFactory factory) : IAsyncLifetime
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected AppDbContext DbContext = null!;
    protected TestDataFactory DataFactory = null!;
    protected Random Random = null!;
        
    private IServiceScope _scope = null!;

    public virtual async Task InitializeAsync()
    {
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
        
        DataFactory = new TestDataFactory();
        Random = new Random();
        
        await factory.InitializeAsync();
    }

    public virtual Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
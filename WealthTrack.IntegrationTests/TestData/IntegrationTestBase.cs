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
        await factory.InitializeAsync();

        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DataFactory = new TestDataFactory();
        Random = new Random();
    }

    public virtual async Task DisposeAsync()
    {
        _scope.Dispose();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Goals.RemoveRange(db.Goals);
        db.Currencies.RemoveRange(db.Currencies);
        db.Categories.RemoveRange(db.Categories);
        db.Budgets.RemoveRange(db.Budgets);
        db.Wallets.RemoveRange(db.Wallets);
        db.Transactions.RemoveRange(db.Transactions);
        db.TransferTransactions.RemoveRange(db.TransferTransactions);

        await db.SaveChangesAsync();
    }
}
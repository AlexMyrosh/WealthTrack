using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WealthTrack.API;
using WealthTrack.Data.Context;

namespace WealthTrack.IntegrationTests.WebAppFactories.Base;

public abstract class BaseTestWebAppFactory : WebApplicationFactory<Program>
{
    public IConfiguration Configuration = null!;
    private const string TestEnvironmentName = "Testing";
    private const string ConfigurationName = "appsettings.Development.json";
    private const string ConnectionStringName = "IntegrationTestsConnection";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(TestEnvironmentName);
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddJsonFile(ConfigurationName);
            Configuration = configBuilder.Build();
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString(ConnectionStringName);
                options.UseSqlite(connectionString);
            });
        });
    }

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}
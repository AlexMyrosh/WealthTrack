using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WealthTrack.API;
using WealthTrack.Data.Context;

public abstract class BaseTestWebAppFactory : WebApplicationFactory<Program>
{
    protected IConfiguration _configuration = default!;
    protected IServiceProvider _serviceProvider = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.Development.json", optional: true);
            _configuration = configBuilder.Build();
        });

        builder.ConfigureServices(services =>
        {
            // Перерегистрировать DbContext
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                var conn = _configuration.GetConnectionString("IntegrationTestsConnection");
                options.UseSqlServer(conn);
            });

            _serviceProvider = services.BuildServiceProvider();
        });
    }

    public virtual async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using WealthTrack.API.AutoMapper;
using WealthTrack.API.Middlewares;
using WealthTrack.Business.AutoMapper;
using WealthTrack.Business.Seeders;
using WealthTrack.Business.Services.Implementations;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Data.Context;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();

            app.UseMiddleware<ApiKeyValidationMiddleware>();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var currencySeeder = scope.ServiceProvider.GetRequiredService<CurrencySeeder>();
                await currencySeeder.SeedCurrenciesAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddOpenApi();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            services.AddAutoMapper(typeof(DomainAndBusinessModelsMapperProfile));
            services.AddAutoMapper(typeof(BusinessAndApiModelsMapperProfile));

            services.AddTransient<ApiKeyValidationMiddleware>();

            services.AddHttpClient<CurrencySeeder>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<IGoalService, GoalService>();

            services.AddScoped<CurrencySeeder>();
        }
    }
}
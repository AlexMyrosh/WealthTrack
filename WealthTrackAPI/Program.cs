using Microsoft.EntityFrameworkCore;
using WealthTrack.API.AutoMapper;
using WealthTrack.API.Middlewares;
using WealthTrack.Business.AutoMapper;
using WealthTrack.Business.EventHandlers.CategoryDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.GoalCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.GoalUpdatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransactionCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionUpdatedEventHandlers;
using WealthTrack.Business.EventHandlers.WalletCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.WalletDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.WalletUpdatedEventHandlers;
using WealthTrack.Business.Events;
using WealthTrack.Business.Events.Interfaces;
using WealthTrack.Business.Events.Models;
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

            ConfigureMiddleware(app);

            if (!app.Environment.IsEnvironment("Testing"))
            {
                await EnsureDatabaseCreatedAndSeededAsync(app.Services);
            }

            app.Run();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddOpenApi();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<DomainAndBusinessModelsMapperProfile>();
                cfg.AddProfile<BusinessAndApiModelsMapperProfile>();
            });

            services.AddTransient<ApiKeyValidationMiddleware>();

            services.AddHttpClient<CurrenciesSeeder>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<IGoalService, GoalService>();

            services.AddScoped<CurrenciesSeeder>();
            services.AddScoped<SystemCategoriesSeeder>();

            services.AddScoped<IEventHandler<TransactionCreatedEvent>, WalletUpdateOnTransactionCreationEventHandler>();
            services.AddScoped<IEventHandler<TransactionCreatedEvent>, GoalUpdateOnTransactionCreationEventHandler>();
            services.AddScoped<IEventHandler<TransactionUpdatedEvent>, WalletUpdateOnTransactionUpdateEventHandler>();
            services.AddScoped<IEventHandler<TransactionUpdatedEvent>, GoalUpdateOnTransactionUpdateEventHandler>();
            services.AddScoped<IEventHandler<TransactionDeletedEvent>, WalletUpdateOnTransactionDeletionEventHandler>();
            services.AddScoped<IEventHandler<TransactionDeletedEvent>, GoalUpdateOnTransactionDeletionEventHandler>();

            services.AddScoped<IEventHandler<TransferTransactionCreatedEvent>, WalletUpdateOnTransferTransactionCreateEventHandler>();
            services.AddScoped<IEventHandler<TransferTransactionUpdatedEvent>, WalletUpdateOnTransferTransactionUpdateEventHandler>();
            services.AddScoped<IEventHandler<TransferTransactionDeletedEvent>, WalletUpdateOnTransferTransactionDeletionEventHandler>();

            services.AddScoped<IEventHandler<WalletCreatedEvent>, BudgetUpdateOnWalletCreationEventHandler>();
            services.AddScoped<IEventHandler<WalletUpdatedEvent>, BudgetUpdateOnWalletUpdateEventHandler>();
            services.AddScoped<IEventHandler<WalletDeletedEvent>, BudgetUpdateOnWalletDeletionEventHandler>();
            
            services.AddScoped<IEventHandler<GoalUpdatedEvent>, GoalUpdateOnGoalUpdateEventHandler>();
            services.AddScoped<IEventHandler<GoalCreatedEvent>, GoalUpdateOnGoalCreationEventHandler>();

            services.AddScoped<IEventHandler<CategoryDeletedEvent>, GoalUpdateOnCategoryDeletionEventHandler>();

            services.AddScoped<IEventPublisher, EventPublisher>();
        }

        private static async Task EnsureDatabaseCreatedAndSeededAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var currencySeeder = scope.ServiceProvider.GetRequiredService<CurrenciesSeeder>();
            await currencySeeder.SeedAsync();
            
            var categorySeeder = scope.ServiceProvider.GetRequiredService<SystemCategoriesSeeder>();
            await categorySeeder.SeedAsync();
        }
    }
}
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using WealthTrack.API.AutoMapper;
using WealthTrack.API.FluentValidationRules;
using WealthTrack.API.Middlewares;
using WealthTrack.Business.AutoMapper;
using WealthTrack.Business.EventHandlers.TransactionCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransactionDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.TransactionUpdatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionCreatedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionDeletedEventHandlers;
using WealthTrack.Business.EventHandlers.TransferTransactionUpdatedEventHandlers;
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
            app.UseMiddleware<ExceptionHandlingMiddleware>();
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
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString).LogTo(Console.WriteLine, LogLevel.Information));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<DomainAndBusinessModelsMapperProfile>();
                cfg.AddProfile<BusinessAndApiModelsMapperProfile>();
            });
            
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
            
            services.AddValidatorsFromAssemblyContaining<BudgetUpsertApiModelValidator>();
            
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();

            services.AddScoped<IEventHandler<TransactionCreatedEvent>, WalletUpdateOnTransactionCreationEventHandler>();
            services.AddScoped<IEventHandler<TransactionUpdatedEvent>, WalletUpdateOnTransactionUpdateEventHandler>();
            services.AddScoped<IEventHandler<TransactionDeletedEvent>, WalletUpdateOnTransactionDeletionEventHandler>();

            services.AddScoped<IEventHandler<TransferTransactionCreatedEvent>, WalletUpdateOnTransferTransactionCreateEventHandler>();
            services.AddScoped<IEventHandler<TransferTransactionUpdatedEvent>, WalletUpdateOnTransferTransactionUpdateEventHandler>();
            services.AddScoped<IEventHandler<TransferTransactionDeletedEvent>, WalletUpdateOnTransferTransactionDeletionEventHandler>();

            services.AddScoped<IEventPublisher, EventPublisher>();
        }

        private static async Task EnsureDatabaseCreatedAndSeededAsync(IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var currencySeeder = scope.ServiceProvider.GetRequiredService<CurrenciesSeeder>();
            await currencySeeder.SeedAsync();
            
            var categorySeeder = scope.ServiceProvider.GetRequiredService<SystemCategoriesSeeder>();
            await categorySeeder.SeedAsync();
        }
    }
}
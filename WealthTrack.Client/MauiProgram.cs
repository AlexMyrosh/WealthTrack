using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using WealthTrack.Client.Models;
using WealthTrack.Client.Services;
using WealthTrack.Client.Services.Implementations;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views;
using WealthTrack.Data.Context;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WealthTrack.Client.appsettings.json");
        var config = new ConfigurationBuilder().AddJsonStream(stream!).Build();
        builder.Configuration.AddConfiguration(config);

        ConfigureServices(builder.Services, config);
        
        var oauthSettings = builder.Configuration.GetSection("OAuth").Get<OAuthSettings>();
        if (oauthSettings is null)
        {
            throw new Exception("OAuth settings not found");
        }

        builder.Services.AddSingleton(oauthSettings);
        
        builder.Services.AddHttpClient<IAuthService, AuthService>(client => { client.BaseAddress = new Uri(oauthSettings.BackendBaseUrl); })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // TODO: REMOVE BEFORE DEPLOYING TO PROD!
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        builder.UseMauiApp<App>().UseMauiCommunityToolkit().ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        var app = builder.Build();
        EnsureDatabaseCreated(app.Services);
        
        return app;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString).LogTo(Console.WriteLine, LogLevel.Information));

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DomainAndBusinessModelsMapperProfile>();
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

        services.AddScoped<IEventHandler<TransactionCreatedEvent>, WalletUpdateOnTransactionCreationEventHandler>();
        services.AddScoped<IEventHandler<TransactionUpdatedEvent>, WalletUpdateOnTransactionUpdateEventHandler>();
        services.AddScoped<IEventHandler<TransactionDeletedEvent>, WalletUpdateOnTransactionDeletionEventHandler>();

        services.AddScoped<IEventHandler<TransferTransactionCreatedEvent>, WalletUpdateOnTransferTransactionCreateEventHandler>();
        services.AddScoped<IEventHandler<TransferTransactionUpdatedEvent>, WalletUpdateOnTransferTransactionUpdateEventHandler>();
        services.AddScoped<IEventHandler<TransferTransactionDeletedEvent>, WalletUpdateOnTransferTransactionDeletionEventHandler>();

        services.AddScoped<IEventPublisher, EventPublisher>();
        
        services.AddSingleton<ApiClient>();

        // Pages
        services.AddTransient<MainPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<SignUpPage>();
        services.AddTransient<LoadingPage>();
    }
    
    private static void EnsureDatabaseCreated(IServiceProvider services)
    {
        using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}
using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.Maui.SegmentedControl;
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
using WealthTrack.Client.AutoMapper;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services;
using WealthTrack.Client.Services.Implementations;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views;
using WealthTrack.Client.ViewModels.Onboarding;
using WealthTrack.Client.Views.Account;
using WealthTrack.Client.Views.Configuration;
using WealthTrack.Client.Views.Goal;
using WealthTrack.Client.Views.Onboarding;
using WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;
using WealthTrack.Client.Views.Transaction;
using WealthTrack.Client.Views.Wallet;
using WealthTrack.Data.Context;
using WealthTrack.Data.UnitOfWork;

namespace WealthTrack.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "WealthTrackDb.db");
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
        builder.Services.AddHttpClient<ISyncService, SyncService>(client => { client.BaseAddress = new Uri(oauthSettings.BackendBaseUrl); })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // TODO: REMOVE BEFORE DEPLOYING TO PROD!
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

        builder.UseMauiApp<App>().UseMauiCommunityToolkit().UseSegmentedControl().ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        var app = builder.Build();
        EnsureDatabaseCreated(app.Services);

        return app;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var dbFileName = configuration["DbName"]!;
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbFileName);
            options.UseSqlite($"Data Source={dbPath}").LogTo(Console.WriteLine, LogLevel.Information);
        });
        
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DomainAndBusinessModelsMapperProfile>();
            cfg.AddProfile<BusinessAndClientModelsMapperProfile>();
        });

        services.AddHttpClient<CurrenciesSeeder>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IGoalService, GoalService>();

        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<INavigationService, NavigationService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<CurrenciesSeeder>();
        services.AddScoped<SystemCategoriesSeeder>();

        services.AddScoped<IEventHandler<TransactionCreatedEvent>, WalletUpdateOnTransactionCreationEventHandler>();
        services.AddScoped<IEventHandler<TransactionUpdatedEvent>, WalletUpdateOnTransactionUpdateEventHandler>();
        services.AddScoped<IEventHandler<TransactionDeletedEvent>, WalletUpdateOnTransactionDeletionEventHandler>();

        services.AddScoped<IEventHandler<TransferTransactionCreatedEvent>, WalletUpdateOnTransferTransactionCreateEventHandler>();
        services.AddScoped<IEventHandler<TransferTransactionUpdatedEvent>, WalletUpdateOnTransferTransactionUpdateEventHandler>();
        services.AddScoped<IEventHandler<TransferTransactionDeletedEvent>, WalletUpdateOnTransferTransactionDeletionEventHandler>();

        services.AddScoped<IEventPublisher, EventPublisher>();
        
        // Account pages
        services.AddTransient<AccountCreationPage>();
        services.AddTransient<ForgotPasswordPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ResetPasswordPage>();
        
        // Configuration pages
        services.AddTransient<ConfigurationPage>();
        
        // Goal pages
        services.AddTransient<GoalsPage>();
        
        // Onboarding pages
        services.AddTransient<CurrencySelectionPage>();
        services.AddTransient<InitialWalletCreationPage>();
        services.AddTransient<SyncSelectionPage>();
        services.AddTransient<ThemeSelectionPage>();
        services.AddTransient<AppFeaturesCarouselPage>();
        
        // Transaction pages
        services.AddTransient<TransactionsPage>();
        
        // Wallet pages
        services.AddTransient<WalletsPage>();
        
        // Other pages
        services.AddTransient<LoadingPage>();
        
        // ViewModels
        services.AddTransient<AppFeaturesCarouselViewModel>();
    }

    private static void EnsureDatabaseCreated(IServiceProvider services)
    {
        using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SecureStorage.RemoveAll();
        //dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}
using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services;
using WealthTrack.Client.Services.Implementations;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels;
using WealthTrack.Client.Views;

namespace WealthTrack.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WealthTrack.Client.appsettings.json");
        var config = new ConfigurationBuilder().AddJsonStream(stream!).Build();
        builder.Configuration.AddConfiguration(config);

        // Загружаем OAuthSettings и регистрируем в DI
        var oauthSettings = builder.Configuration.GetSection("OAuth").Get<OAuthSettings>();
        builder.Services.AddSingleton(oauthSettings);

        // AuthService с HttpClient
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(oauthSettings.BackendBaseUrl);
        });

        // Остальные сервисы
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddTransient<IBudgetService, BudgetService>();
        builder.Services.AddTransient<IWalletService, WalletService>();
        builder.Services.AddTransient<ITransactionService, TransactionService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();

        builder.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
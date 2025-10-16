using AutoMapper;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

public partial class ThemeSelectionPage : ContentPage
{
    public ThemeSelectionPage(IUserService userService, IThemeService themeService, IWalletService walletService, ICurrencyService currencyService, INavigationService navigationService, IMapper mapper, IDialogService dialogService)
    {
        InitializeComponent();
        BindingContext = new ThemeSelectionViewModel(userService, themeService, walletService, currencyService, Navigation, navigationService, mapper, dialogService);
    }
}
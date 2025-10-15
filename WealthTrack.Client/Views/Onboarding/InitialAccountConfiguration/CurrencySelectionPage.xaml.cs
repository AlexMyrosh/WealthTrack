using AutoMapper;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

public partial class CurrencySelectionPage
{
    public CurrencySelectionPage(IUserService userService, ICurrencyService currencyService, IWalletService walletService, IThemeService themeService, IMapper mapper)
    {
        InitializeComponent();
        var viewModel = new CurrencySelectionViewModel(userService, currencyService, walletService, themeService, mapper);
        BindingContext = viewModel;
        viewModel.Navigation = Navigation;
    }
}
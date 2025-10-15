using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

public partial class SyncSelectionPage
{
    public SyncSelectionPage(IUserService userService, IWalletService walletService, ICurrencyService currencyService)
    {
        InitializeComponent();
        BindingContext = new SyncSelectionViewModel(userService, walletService, currencyService, Navigation);
    }
}
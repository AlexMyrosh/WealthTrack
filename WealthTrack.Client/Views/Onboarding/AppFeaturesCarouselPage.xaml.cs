using WealthTrack.Client.ViewModels.Onboarding;

namespace WealthTrack.Client.Views.Onboarding;

public partial class AppFeaturesCarouselPage
{
    public AppFeaturesCarouselPage()
    {
        InitializeComponent();
        BindingContext = new AppFeaturesCarouselViewModel();
    }
}
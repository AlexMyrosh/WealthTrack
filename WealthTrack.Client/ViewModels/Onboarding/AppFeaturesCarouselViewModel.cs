using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Client.Models;

namespace WealthTrack.Client.ViewModels.Onboarding;

public partial class AppFeaturesCarouselViewModel : ObservableObject
{
    
    [ObservableProperty] private int _currentIndex;
    [ObservableProperty] private AppFeatureModel _currentFeature;

    public ObservableCollection<AppFeatureModel> Features { get; }

    public ICommand GetStartedCommand { get; }

    public AppFeaturesCarouselViewModel()
    {
        Features =
        [
            new AppFeatureModel { Title = "Welcome to WealthTrack", Description = "Your personal finance companion designed to help you take control of your money and build lasting wealth." },
            new AppFeatureModel { Title = "Track Your Spending", Description = "Monitor your expenses with detailed categorization and real-time insights into where your money goes." },
            new AppFeatureModel { Title = "Set Financial Goals", Description = "Create and track your financial objectives, from emergency funds to major purchases and retirement planning." },
            new AppFeatureModel { Title = "Analyze Your Progress", Description = "Get comprehensive insights into your financial health with detailed reports and trend analysis." },
            new AppFeatureModel { Title = "Secure & Private", Description = "Your financial data is protected with bank-level security. We never share your personal information." },
            new AppFeatureModel { Title = "Ready to Get Started?", Description = "Sign in to your account or create a new one to begin your wealth tracking journey." }
        ];

        CurrentFeature = Features.First();
        GetStartedCommand = new AsyncRelayCommand(OnGetStarted);
    }
    
    partial void OnCurrentIndexChanged(int value)
    {
        if (value >= 0 && value < Features.Count)
        {
            CurrentFeature = Features[value];
        }
    }

    private async Task OnGetStarted()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
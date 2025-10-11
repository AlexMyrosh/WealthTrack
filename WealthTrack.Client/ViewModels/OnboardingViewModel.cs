using System.Collections.ObjectModel;
using System.Windows.Input;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.ViewModels;

public class OnboardingViewModel : BaseViewModel
{
    private int _currentIndex = 0;
    private OnboardingItem _currentItem = new();

    public OnboardingViewModel()
    {
        InitializeOnboardingItems();
        SkipCommand = new Command(() => SkipToLogin());
        NextCommand = new Command(() => NavigateToNext());
        PreviousCommand = new Command(() => NavigateToPrevious());
        UpdateCurrentItem();
    }

    public ObservableCollection<OnboardingItem> OnboardingItems { get; } = new();
    
    public int TotalItems => OnboardingItems.Count;

    public OnboardingItem CurrentItem
    {
        get => _currentItem;
        set
        {
            _currentItem = value;
            OnPropertyChanged();
        }
    }

    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            _currentIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowLeftArrow));
            OnPropertyChanged(nameof(ShowRightArrow));
            OnPropertyChanged(nameof(ShowNavigation));
            UpdateCurrentItem();
        }
    }

    public bool ShowLeftArrow => _currentIndex > 0;
    
    public bool ShowRightArrow => _currentIndex < OnboardingItems.Count - 1;
    
    public bool ShowNavigation => !CurrentItem?.IsLoginPage == true;

    public ICommand SkipCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }

    private void InitializeOnboardingItems()
    {
        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Welcome to WealthTrack",
            Description = "Your personal finance companion designed to help you take control of your money and build lasting wealth.",
            ShowSkipButton = true
        });

        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Track Your Spending",
            Description = "Monitor your expenses with detailed categorization and real-time insights into where your money goes.",
            ShowFeatures = true,
            ShowSkipButton = true
        });

        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Set Financial Goals",
            Description = "Create and track your financial objectives, from emergency funds to major purchases and retirement planning.",
            ShowFeatures = true,
            ShowSkipButton = true
        });

        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Analyze Your Progress",
            Description = "Get comprehensive insights into your financial health with detailed reports and trend analysis.",
            ShowFeatures = true,
            ShowSkipButton = true
        });

        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Secure & Private",
            Description = "Your financial data is protected with bank-level security. We never share your personal information.",
            ShowFeatures = true,
            ShowSkipButton = true
        });

        // Add login page as the last item
        OnboardingItems.Add(new OnboardingItem
        {
            Title = "Ready to Get Started?",
            Description = "Sign in to your account or create a new one to begin your wealth tracking journey.",
            ShowSkipButton = false,
            IsLoginPage = true
        });
    }


    private void SkipToLogin()
    {
        // Go directly to the last item (login page)
        CurrentIndex = OnboardingItems.Count - 1;
    }

    private async Task NavigateToLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    private void UpdateCurrentItem()
    {
        if (_currentIndex < OnboardingItems.Count)
        {
            CurrentItem = OnboardingItems[_currentIndex];
        }
    }

    public void OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        var item = e.CurrentItem as OnboardingItem;
        if (item != null)
        {
            var index = OnboardingItems.IndexOf(item);
            if (index != -1)
            {
                CurrentIndex = index;
            }
        }
    }

    public void NavigateToNext()
    {
        if (_currentIndex < OnboardingItems.Count - 1)
        {
            CurrentIndex = _currentIndex + 1;
        }
        else if (CurrentItem?.IsLoginPage == true)
        {
            NavigateToLogin();
        }
    }

    public void NavigateToPrevious()
    {
        if (_currentIndex > 0)
        {
            CurrentIndex = _currentIndex - 1;
        }
    }
}

using WealthTrack.Client.ViewModels;

namespace WealthTrack.Client.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;
    private bool _isUpdatingPosition = false;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        // Wire up the carousel events
        OnboardingCarousel.CurrentItemChanged += OnCurrentItemChanged;
        OnboardingCarousel.PositionChanged += OnPositionChanged;
        
        // Subscribe to view model property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OnboardingViewModel.CurrentIndex) && !_isUpdatingPosition)
        {
            _isUpdatingPosition = true;
            OnboardingCarousel.Position = _viewModel.CurrentIndex;
            _isUpdatingPosition = false;
        }
    }

    private void OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
    {
        if (!_isUpdatingPosition)
        {
            _viewModel.OnCurrentItemChanged(sender, e);
        }
    }

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        if (!_isUpdatingPosition)
        {
            // Prevent going beyond the bounds
            if (e.CurrentPosition >= _viewModel.TotalItems)
            {
                _isUpdatingPosition = true;
                OnboardingCarousel.Position = _viewModel.TotalItems - 1;
                _isUpdatingPosition = false;
            }
            else if (e.CurrentPosition < 0)
            {
                _isUpdatingPosition = true;
                OnboardingCarousel.Position = 0;
                _isUpdatingPosition = false;
            }
            else if (e.CurrentPosition != _viewModel.CurrentIndex)
            {
                _viewModel.CurrentIndex = e.CurrentPosition;
            }
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ensure we start at the first item
        _isUpdatingPosition = true;
        OnboardingCarousel.Position = 0;
        _isUpdatingPosition = false;
    }
}

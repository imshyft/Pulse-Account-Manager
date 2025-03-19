using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;

using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Services.Storage;
using Wpf.Ui.Controls;

namespace Studio.Views;

// TODO : Add the Template Studio Theming package, and try to make it change wpf-ui theme
public partial class ShellWindow : FluentWindow, IShellWindow, INotifyPropertyChanged
{
    public static ShellWindow Instance { get; private set; }
    public ContentPresenter DialogPresenter => RootContentDialogPresenter;
    public ObservableCollection<object> MenuItems { get; private set; } = new()
    {
        new NavigationViewItem()
        {
            Content = "Home",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Person24 },
            TargetPageType = typeof(MainPage)
        },
        new NavigationViewItem()
        {
            Content = "Patch",
            Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
            TargetPageType = typeof(PatchNotesPage)
        }
    };

    public ObservableCollection<object> FooterItems { get; private set; } = new()
    {
        new NavigationViewItem()
        {
            Content = "Settings",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
            TargetPageType = typeof(SettingsPage)
        }
    };

    private readonly INavigationService _navigationService;
    private bool _canGoBack;

    public bool CanGoBack
    {
        get { return _canGoBack; }
        set { Set(ref _canGoBack, value); }
    }

    public ShellWindow(INavigationService navigationService, UserProfileDataService profileDataService)
    {
        _navigationService = navigationService;
        InitializeComponent();
        DataContext = this;
        Instance = this;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _navigationService.Navigated += OnNavigated;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _navigationService.Navigated -= OnNavigated;
    }

    private void OnNavigated(object sender, Type pageType)
    {
        CanGoBack = _navigationService.CanGoBack;
    }

    private void OnGoBack(object sender, RoutedEventArgs e)
    {
        _navigationService.GoBack();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void NavigateTo(Type targetPage)
    {
        if (targetPage != null)
        {
            _navigationService.NavigateTo(targetPage);
        }
    }

    private void OnNavigating(NavigationView sender, RoutedEventArgs args)
    {
        NavigatingCancelEventArgs navArgs = args as NavigatingCancelEventArgs;
        navArgs.Cancel = true;
        Page page = (Page)navArgs.Page;
        NavigateTo(page.GetType());
        
        
    }
}

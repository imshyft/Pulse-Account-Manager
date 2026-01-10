using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Controls;
using Studio.Dialogs;
using Studio.Models;
using Studio.Services;
using Studio.Services.Data;
using Studio.Services.Files;
using Studio.Services.Storage;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static SkiaSharp.HarfBuzz.SKShaper;
using Button = Wpf.Ui.Controls.Button;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views;


public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{


    // TODO : Add flyouts to introduce main actions (adding profiles, favourites) on first time launch
    public ObservableCollection<ProfileV2> FilteredProfiles { get; set; } = new();
    public GroupSelectionService GroupSelectionService { get; set; }
    public bool IsPanelShowingFavourites { get; set; } = true;

    private readonly FavouriteProfileDataService _favouriteProfiles;
    private readonly AccountActionsService _accountActionsService;
    private readonly BattleNetService _battleNetService;
    private readonly UserProfileDataService _userProfiles;
    private readonly CustomSnackbarService _snackbarService;
    private readonly IAppPaths _appPaths;

    private readonly int favouritesPanelExpandedWidth = 250;
    private readonly int favouritesPanelCollapsedWidth = 75;

    private readonly PersistAndRestoreService _persistAndRestoreService;

    public bool IsFavouritesPanelCollapsed
    {
        get => _isFavouritesPanelCollapsed;
        set => Set(ref _isFavouritesPanelCollapsed, value);
    }
    private bool _isFavouritesPanelCollapsed = false;

        
    public MainPage()
    {
        InitializeComponent();
        DataContext = this;
        _accountActionsService = ((App)Application.Current).GetService<AccountActionsService>();
        _favouriteProfiles = ((App)Application.Current).GetService<FavouriteProfileDataService>();
        _userProfiles = ((App)Application.Current).GetService<UserProfileDataService>();
        GroupSelectionService = ((App)Application.Current).GetService<GroupSelectionService>();
        _battleNetService = ((App)Application.Current).GetService<BattleNetService>();
        _snackbarService = ((App)Application.Current).GetService<CustomSnackbarService>();
        _appPaths = ((App)Application.Current).GetService<IAppPaths>();
        
        
        _snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        foreach (var profile in _favouriteProfiles.Profiles)
        {
            FilteredProfiles.Add(profile);
        }

        _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();

        OpenAccountList();
        
        PreviewMouseDown += OnPreviewMouseDown;

        bool isFirstLaunch = _persistAndRestoreService.GetValue("IsFirstLaunch", true);
        if (isFirstLaunch)
        {
            _ = ShowFirstLaunchDialog();
        }
        else
        {
            _ = validatePaths();
        }
    }

    private async Task validatePaths()
    {
        if (!_battleNetService.IsPathsValid())
        {
            if (ShellWindow.Instance == null)
                return;

            ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
            if (dialogPresenter == null)
                return;

            var dialog = new SettingsReviewDialog(dialogPresenter,
                "Some settings may be invalid or outdated. Please review the settings here.");

            var result = await dialog.ShowAsync();
        }
    }
    private async Task ShowFirstLaunchDialog()
    {
        bool isFirstLaunch = _persistAndRestoreService.GetValue("IsFirstLaunch", true);
        if (isFirstLaunch)
        {
            if (ShellWindow.Instance == null)
                return;

            ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
            if (dialogPresenter == null)
                return;

            var dialog = new SettingsReviewDialog(dialogPresenter,
                "Welcome! To get started, review these settings here, and then start adding accounts with the + button.");
            var result = await dialog.ShowAsync();

            _persistAndRestoreService.SetValue("IsFirstLaunch", false);
            _persistAndRestoreService.PersistData();

        }
    }
    

    #region Property Handling

    public event PropertyChangedEventHandler PropertyChanged;
    private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Equals(storage, value))
        {
            return;
        }

        storage = value;
        OnPropertyChanged(propertyName);
    }
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion

    #region Navigation
    public void OnNavigatedTo(object parameter)
    {


    }

    public void OnNavigatedFrom()
    {
        return;
    }


    public void OpenDetailsPage(ProfileV2 profile)
    {
        mainContentFrame.NavigationService.Navigate(new AccountDetailsPage(profile));

    }
    private void OpenAccountList()
    {
        FavouritesList.SelectedItem = null;
        mainContentFrame.NavigationService.Navigate(new AccountListPage());
    }
#endregion


    protected void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (mainContentFrame.CanGoBack && e.ChangedButton == MouseButton.XButton1)
        {
            e.Handled = true;
            mainContentFrame.GoBack();
        }
        else if (mainContentFrame.CanGoForward && e.ChangedButton == MouseButton.XButton2)
        {
            e.Handled = true;
            mainContentFrame.GoForward();
        }

        base.OnPreviewMouseDown(e);
    }

    // Handle Sidebar Clicking as LeftButtonUps on each list item so it doesnt trigger on rmb
    private void FavouritesList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var control = (FrameworkElement)sender;
        if (control.DataContext is ProfileV2 profile)
        {
            OpenDetailsPage(profile);

        }
    }

    private void OnAccountListButtonClick(object sender, RoutedEventArgs e)
    {
        OpenAccountList();
    }

    private void OnFavouritesSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        string text = FilterTextBox.Text;

        ProfileDataService profileSource = IsPanelShowingFavourites ? _favouriteProfiles : _userProfiles;
        var filtered = profileSource.Profiles.Where(p => p.CustomName.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(p => p.Battletag.ToString());


        FilteredProfiles.Clear();

        foreach (var item in filtered)
        {
            FilteredProfiles.Add(item);
        }
    }
    
    private void OnToggleOpenFavouritesPanelButtonClick(object sender, RoutedEventArgs e)
    {


        FavouritesColumn.MaxWidth = IsFavouritesPanelCollapsed ? favouritesPanelExpandedWidth : favouritesPanelCollapsedWidth;

        PanelCollapseButtonSymbol.Symbol = IsFavouritesPanelCollapsed
            ? SymbolRegular.ChevronDoubleRight20
            : SymbolRegular.ChevronDoubleLeft20;

        IsFavouritesPanelCollapsed = !IsFavouritesPanelCollapsed;

        
    }

    private async void OnAddUserProfileButtonClick(object sender, RoutedEventArgs e)
    {
        if (ShellWindow.Instance == null)
            return;

        ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
        if (dialogPresenter == null)
            return;


        var dialog = new AddAccountPrompt(dialogPresenter);
        

        // Show the dialog and wait for user input
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ProfileV2 profile = dialog.Profile;
            string email = _battleNetService.GetBattleNetAccount();
            profile.Email = email;
            _userProfiles.SaveProfile(profile);
        }
        RefreshPanel();
    }
    private async void OnAddFavouriteProfileButtonClick(object sender, RoutedEventArgs e)
    {
        if (ShellWindow.Instance == null)
            return;

        ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
        if (dialogPresenter == null)
            return;


        var dialog = new AddFavouriteProfilePrompt(dialogPresenter);


        // Show the dialog and wait for user input
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ProfileV2 profile = dialog.Profile;

            if (_favouriteProfiles.ContainsProfile(profile))
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    Appearance = ControlAppearance.Danger,
                    Title = "Profile already exists!",
                    Content = "You have already added this profile before, so we'll just update it",
                    Icon = new SymbolIcon(SymbolRegular.Info16),
                });
            }

            _favouriteProfiles.SaveProfile(profile);
            RefreshPanel();
        }
    }

    private void OnFavouriteUserToggleButtonClick(object sender, RoutedEventArgs e)
    {
        TogglePanelProfileSource();
    }
    private void TogglePanelProfileSource()
    {
        if (IsPanelShowingFavourites)
        {
            FilteredProfiles.Clear();
            foreach (var profile in _userProfiles.Profiles)
            {
                FilteredProfiles.Add(profile);
            }

            PanelToggleSymbol.Symbol = SymbolRegular.Person48;
        }
        else
        {
            FilteredProfiles.Clear();
            foreach (var profile in _favouriteProfiles.Profiles)
            {
                FilteredProfiles.Add(profile);
            }

            PanelToggleSymbol.Symbol = SymbolRegular.Star48;
        }

        IsPanelShowingFavourites = !IsPanelShowingFavourites;

        RefreshPanel();
    }

    private void OnToggleGroupSelectionButtonClick(object sender, RoutedEventArgs e)
    {
        bool isToggled = (sender as ToggleButton).IsChecked == true;
        GroupSelectionService.IsEnabled = isToggled;



        if (isToggled)
        {
            TogglePanelExpandButton.Opacity = 0.2;
            if (!IsPanelShowingFavourites)
                TogglePanelProfileSource();
            _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Info,
                Opacity = 0.9,
                IsCloseButtonEnabled = false,
                Title = "Group Selection Mode",
                Content = "Click on the ranks of the roles your friends will play on on the side panel to see which accounts are in range",
                Timeout = TimeSpan.FromSeconds(10),
                
                Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
            });
        

        }
        else
        {
            SnackbarPresenter.HideCurrent();
            TogglePanelExpandButton.Opacity = 1;
            GroupSelectionService.RemoveAllMembers();
        }
    }

    private void OnSidePanelRoleButtonClick(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is not ProfileV2 profile)
            return;
        if (((FrameworkElement)sender).Tag is not RoleV2 role)
            return;

        if (GroupSelectionService.Contains(role))
        {
            GroupSelectionService.RemoveMember(role);
        }
        else
        {
            GroupSelectionService.AddMember(role);
        }

        
    }

    private async void OnSidePanelItemSyncClick(object sender, RoutedEventArgs e)
    {
        if (FavouritesList.SelectedItem is not ProfileV2 profile)
            return;

        var updatedProfile = await _accountActionsService.TrySyncAccount(profile);
        if (updatedProfile != null)
        {
            if (IsPanelShowingFavourites)
            {
                _favouriteProfiles.DeleteProfile(profile);
                _favouriteProfiles.SaveProfile(updatedProfile);
            }
            else
            {
                _userProfiles.DeleteProfile(profile);
                _userProfiles.SaveProfile(updatedProfile);
            }
        }


        RefreshPanel();


    }

    private void OnSidePanelItemDeleteClick(object sender, RoutedEventArgs e)
    {
        if (FavouritesList.SelectedItem is not ProfileV2 profile)
            return;

        if (IsPanelShowingFavourites)
        {
            _favouriteProfiles.DeleteProfile(profile);
        }
        else
        {
            _userProfiles.DeleteProfile(profile);
        }

        _snackbarService.Show(true, s =>
        {
            s.Appearance = ControlAppearance.Success;
            s.Title = "Account deleted";
            s.Icon = new SymbolIcon(SymbolRegular.Checkmark16);
        });

        RefreshPanel();
    }

    private void OnOpenConfigDirClick(object sender, RoutedEventArgs e)
    {
        var startInfo = new ProcessStartInfo("explorer.exe", _appPaths.Root);
        Process.Start(startInfo);
    }

    private async void OnReviewSettingsClick(object sender, RoutedEventArgs e)
    {
        ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
        if (dialogPresenter == null)
            return;

        var dialog = new SettingsReviewDialog(dialogPresenter,
            "Review the current settings here (Full Settings page coming soon)");
        await dialog.ShowAsync();
    }

}

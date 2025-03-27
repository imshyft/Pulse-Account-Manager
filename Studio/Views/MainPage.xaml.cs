using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using Wpf.Ui.Controls;
using static SkiaSharp.HarfBuzz.SKShaper;
using Button = Wpf.Ui.Controls.Button;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views;


public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{


    // TODO : Add flyouts to introduce main actions (adding profiles, favourites) on first time launch
    public ObservableCollection<Profile> FilteredProfiles { get; set; } = new();
    public GroupSelectionService GroupSelectionService { get; set; }
    public bool IsPanelShowingFavourites { get; set; } = true;

    private readonly FavouriteProfileDataService _favouriteProfiles;
    private readonly BattleNetService _battleNetService;
    private readonly UserProfileDataService _userProfiles;
    private IProfileFetchingService _profileDataFetchingService;

    private readonly PersistAndRestoreService _persistAndRestoreService;
    private bool IsFavouritesPanelCollapsed { get; set; } = false;

    private int _flyoutOpenId = 0;
    private Flyout[] _flyouts;

    public MainPage()
    {
        InitializeComponent();
        DataContext = this;
        
        _favouriteProfiles = ((App)Application.Current).GetService<FavouriteProfileDataService>();
        _userProfiles = ((App)Application.Current).GetService<UserProfileDataService>();
        GroupSelectionService = ((App)Application.Current).GetService<GroupSelectionService>();
        _profileDataFetchingService = ((App)Application.Current).GetService<IProfileFetchingService>();
        _battleNetService = ((App)Application.Current).GetService<BattleNetService>();



        foreach (var profile in _favouriteProfiles.Profiles)
        {
            FilteredProfiles.Add(profile);
        }

        _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();

        OpenAccountList();
        
        PreviewMouseDown += OnPreviewMouseDown;

        CheckIfFirstLaunch();
        _battleNetService.Initialize();
    }

    private async void CheckIfFirstLaunch()
    {
        bool isFirstLaunch = _persistAndRestoreService.GetValue("IsFirstLaunch", true);
        if (isFirstLaunch)
        {
            if (ShellWindow.Instance == null)
                return;

            ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
            if (dialogPresenter == null)
                return;

            var dialog = new FirstTimePopup(dialogPresenter);

            var result = await dialog.ShowAsync();

            _persistAndRestoreService.SetValue("IsFirstLaunch", false);
            _persistAndRestoreService.PersistData();

            AddProfileDropDownButton.IsDropDownOpen = true;
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


    public void OpenDetailsPage(Profile profile)
    {
        //mainContentFrame.NavigationService.Navigate(new AccountDetailsPage(profile));

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

    private void OnFavouritesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            OpenDetailsPage(e.AddedItems[0] as Profile);
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
        //https://learn.microsoft.com/en-us/windows/apps/design/controls/listview-filtering
        string text = FilterTextBox.Text;

        ProfileDataService profileSource = IsPanelShowingFavourites ? _favouriteProfiles : _userProfiles;
        var filtered = profileSource.Profiles.Where(p => p.CustomId.Contains(text, StringComparison.CurrentCultureIgnoreCase));

        for (int i = FilteredProfiles.Count - 1; i >= 0; i--)
        {
            var item = FilteredProfiles[i];

            if (!filtered.Contains(item))
            {
                FilteredProfiles.Remove(item);
            }
        }

        foreach (var item in filtered)
        {
            if (!FilteredProfiles.Contains(item))
            {
                FilteredProfiles.Add(item);
            }
        }
    }
    private void OnToggleOpenFavouritesPanelButtonClick(object sender, RoutedEventArgs e)
    {
            int expandedWidth = 250;
            int collapsedWidth = 75;

            Storyboard storyboard = new Storyboard();

            Duration duration = new Duration(TimeSpan.FromMilliseconds(500));
            CubicEase ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            DoubleAnimation animation = new DoubleAnimation();
            animation.EasingFunction = ease;
            animation.Duration = duration;
            storyboard.Children.Add(animation);

            

            animation.From = IsFavouritesPanelCollapsed ? collapsedWidth : expandedWidth;
            animation.To = IsFavouritesPanelCollapsed ? expandedWidth : collapsedWidth;
            Storyboard.SetTarget(animation, FavouritesColumn);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(ColumnDefinition.MaxWidth)"));

            storyboard.Begin();

            


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
            Profile profile = dialog.Profile;
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
            Profile profile = dialog.Profile;

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
                Appearance = ControlAppearance.Transparent,
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
        if (((FrameworkElement)sender).DataContext is not Profile profile)
            return;
        if (((FrameworkElement)sender).Tag is not Role role)
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
        if (FavouritesList.SelectedItem is not Profile profile)
            return;

        var result = await _profileDataFetchingService.FetchProfileAsync(profile.Battletag);


        if (result.Outcome == ProfileFetchOutcome.Success)
        {
            _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Success,
                Title = "Synced Account",
                Content = "Favourited profile successfully synced",
                Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
            });

            if (IsPanelShowingFavourites)
            {
                _favouriteProfiles.DeleteProfile(profile);
                _favouriteProfiles.SaveProfile(result.Profile);
            }
            else
            {
                _userProfiles.DeleteProfile(profile);
                _userProfiles.SaveProfile(result.Profile);
            }


        }
        else
        {
            _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Danger,
                Title = "Could not fetch account",
                Content = "Please try again later, or re-add the account if it keeps failing",
                Icon = new SymbolIcon(SymbolRegular.ErrorCircle24),
            });
        }

        RefreshPanel();


    }

    private void OnSidePanelItemDeleteClick(object sender, RoutedEventArgs e)
    {
        if (FavouritesList.SelectedItem is not Profile profile)
            return;

        if (IsPanelShowingFavourites)
        {
            _favouriteProfiles.DeleteProfile(profile);
        }
        else
        {
            _userProfiles.DeleteProfile(profile);
        }

        RefreshPanel();
    }


}

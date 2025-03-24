using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Controls;
using Studio.Dialogs;
using Studio.Models;
using Studio.Services.Data;
using Studio.Services.Files;
using Studio.Services.Storage;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views;


public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{
    // TODO : Find somewhere for button to add favourite (maybe move expand/collapse btn?)
    // TODO : Add Escape for back navigation
    // TODO : Add flyouts to introduce main actions (adding profiles, favourites) on first time launch
    public ObservableCollection<UserData> FilteredProfiles { get; set; } = new();
    private bool isPanelShowingFavourites { get; set; } = true;

    private readonly FavouriteProfileDataService _favouriteProfiles;
    private readonly UserProfileDataService _userProfiles;

    private readonly PersistAndRestoreService _persistAndRestoreService;
    private readonly PathResolverService _pathResolverService;
    private bool IsFavouritesPanelCollapsed { get; set; } = false;

    public MainPage()
    {
        InitializeComponent();
        DataContext = this;

        _favouriteProfiles = ((App)Application.Current).GetService<FavouriteProfileDataService>();
        _userProfiles = ((App)Application.Current).GetService<UserProfileDataService>();

        foreach (var profile in _favouriteProfiles.Profiles)
        {
            FilteredProfiles.Add(profile);
        }

        _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();

        OpenAccountList();
        
        PreviewMouseDown += OnPreviewMouseDown;


        CheckIfFirstLaunch();
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


    public void OpenDetailsPage(UserData profile)
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

    private void OnFavouritesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            OpenDetailsPage(e.AddedItems[0] as UserData);
        }

    }

    private void OnAccountListButtonClick(object sender, RoutedEventArgs e)
    {
        OpenAccountList();
    }

    private void OnFavouritesSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterSearchPanel();
    }

    private void FilterSearchPanel()
    {
        //https://learn.microsoft.com/en-us/windows/apps/design/controls/listview-filtering
        string text = FilterTextBox.Text;

        ProfileDataService profileSource = isPanelShowingFavourites ? _favouriteProfiles : _userProfiles;
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
    private void OnFavouritesPanelButtonClick(object sender, RoutedEventArgs e)
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
            UserData profile = dialog.Profile;
            _userProfiles.SaveProfile(profile);
        }
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
            UserData profile = dialog.Profile;
            _favouriteProfiles.SaveProfile(profile);
            FilterSearchPanel();
        }
    }

    private void OnFavouriteUserToggleButtonClick(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        if (isPanelShowingFavourites)
        {
            FilteredProfiles.Clear();
            foreach (var profile in _userProfiles.Profiles)
            {
                FilteredProfiles.Add(profile);
            }

            PanelToggleSymbol.Symbol =SymbolRegular.Person48;
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

        isPanelShowingFavourites = !isPanelShowingFavourites;

        FilterSearchPanel();
    }


}

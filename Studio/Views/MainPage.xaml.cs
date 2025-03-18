using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Studio.Contracts.Views;
using Studio.Controls;
using Studio.Core.Contracts.Services;
using Studio.Core.Models;
using Studio.Services;
using Wpf.Ui.Controls;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views;

public partial class MainPage : Page, INotifyPropertyChanged, INavigationAware
{

    public ObservableCollection<UserData> Profiles { get; set; } = new ObservableCollection<UserData>();
    public ObservableCollection<UserData> FilteredProfiles { get; set; } = new();


    private IProfileDataService _profileDataService;
    private bool isFavouritesPanelCollapsed { get; set; } = false;

    public MainPage()
    {
        InitializeComponent();
        DataContext = this;
        _profileDataService = ((App)Application.Current).GetService<IProfileDataService>(); ;
        OpenAccountList();
        
        PreviewMouseDown += OnPreviewMouseDown;

        Profiles.Clear();
        foreach (var profile in _profileDataService.GetFavouriteProfiles())
        {
            Profiles.Add(profile);
            FilteredProfiles.Add(profile);
        }


    }



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
        mainContentFrame.NavigationService.Navigate(new AccountListPage(_profileDataService));
    }

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
        //https://learn.microsoft.com/en-us/windows/apps/design/controls/listview-filtering
        string text = (sender as TextBox).Text;
        var filtered = Profiles.Where(p => p.Username.Contains(text, StringComparison.CurrentCultureIgnoreCase));

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
        animation.From = isFavouritesPanelCollapsed ? collapsedWidth : expandedWidth;
        animation.To = isFavouritesPanelCollapsed ? expandedWidth : collapsedWidth;
        Storyboard.SetTarget(animation, FavouritesColumn);
        Storyboard.SetTargetProperty(animation, new PropertyPath("(ColumnDefinition.MaxWidth)"));

        storyboard.Begin();

        isFavouritesPanelCollapsed = !isFavouritesPanelCollapsed;
        
    }

    private async void OnAddUserProfileButtonClick(object sender, RoutedEventArgs e)
    {
        if (ShellWindow.Instance == null)
            return;

        ContentPresenter dialogPresenter = ShellWindow.Instance.DialogPresenter;
        if (dialogPresenter == null)
            return;

        var content = (StackPanel)FindResource("DialogContent");
        var textBox = content.Children.OfType<TextBox>().FirstOrDefault();

        


        var dialog = new AddAccountPrompt(dialogPresenter);
        

        // Show the dialog and wait for user input
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {

        }
    }
}

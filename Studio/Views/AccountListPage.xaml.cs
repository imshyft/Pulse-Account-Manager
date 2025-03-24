using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Studio.Contracts.Services;
using Studio.Contracts.Views;
using Studio.Models;
using Studio.Services;
using Studio.Services.Data;
using Studio.Services.Storage;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;
using Flyout = Wpf.Ui.Controls.Flyout;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views
{
    /// <summary>
    /// Interaction logic for AccountListPage.xaml
    /// </summary>
    public partial class AccountListPage : Page
    {

        public UserProfileDataService UserProfiles {get; set; }
        public GroupSelectionService GroupSelectionService { get; set; }

        private BattleNetService _battleNetService;
        private IProfileFetchingService _profileDataFetchingService;

        private bool _mouseOverButton = false;
        private bool _isFlyoutOpen;

        public Visibility RankColumnsVisibility { get; set; }
        public AccountListPage()
        {
            InitializeComponent();
            
            DataContext = this;

            UserProfiles = ((App)Application.Current).GetService<UserProfileDataService>();
            _battleNetService = ((App)Application.Current).GetService<BattleNetService>();
            _profileDataFetchingService = ((App)Application.Current).GetService<IProfileFetchingService>();
            GroupSelectionService = ((App)Application.Current).GetService<GroupSelectionService>();
            
            AccountDataGrid.SelectedItem = null;
        }


        private void OnProfileListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && !_mouseOverButton && !_isFlyoutOpen)
            {
                //NavigationService?.Navigate(new AccountDetailsPage((e.AddedItems[0] as Profile)));
            }

            AccountDataGrid.SelectedItem = null;
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject source)
            {
                var row = VisualTreeHelper.GetParent(source) as DataGridRow;
                if (row != null && !_isFlyoutOpen)
                {
                    //NavigationService?.Navigate(new AccountDetailsPage((row.DataContext as Profile)));
                }
            }
        }


        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            Profile profile = ((FrameworkElement)sender).DataContext as Profile;
            if (profile == null)
                return;
            if (profile.Email == null)
            {
                SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Danger,
                    Title = "Couldn't Launch Account",
                    Content = "No email is associated with this account",
                    Icon = new SymbolIcon(SymbolRegular.ErrorCircle12),
                    Opacity = 0.9
                });
                return;
            }

            SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                AllowDrop = false,
                Appearance = ControlAppearance.Success,
                Title = "Launching Account!",
                Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35),
                Opacity = 0.9
            });

            _battleNetService.OpenBattleNetWithAccount(profile.Email);

            e.Handled = true;

        }


        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e) => _mouseOverButton = true;
        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e) => _mouseOverButton = false;

        private void OnAccountOptionsClicked(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not Profile profile)
                return;

            var row = VisualTreeHelper.GetParent((DependencyObject)sender) as Grid;
            Flyout flyout = (Flyout)row.FindName("OptionsFlyout");
            _isFlyoutOpen = true;
            flyout.Show();

        }

        private void OnOptionsFlyoutClosed(Flyout sender, RoutedEventArgs args)
        {
            _isFlyoutOpen = false;
        }

        // TODO : make a better sync system that preserves history
        private async void OnOptionsSyncButtonClick(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not Profile profile)
                return;

            var result = await _profileDataFetchingService.GetUserProfile(profile.Battletag);
            SnackbarPresenter.AddToQue(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Info,
                Title = "Syncing Account",
                Content = "Please wait a moment.",
                Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
            });

            if (string.IsNullOrEmpty(result.Error))
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    Appearance = ControlAppearance.Success,
                    Title = "Synced Account",
                    Content = "Account profile successfully synced",
                    Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
                });
                UserProfiles.DeleteProfile(profile);
                UserProfiles.SaveProfile(result.Profile);
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
        }

        private void OnOptionsRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not Profile profile)
                return;

            UserProfiles.DeleteProfile(profile);
            SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Success,
                Title = "Account deleted",
                Icon = new SymbolIcon(SymbolRegular.Checkmark16),
            });
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = e.NewSize.Width;
            for (int i = 2; i < 5; i++)
            {
                if (width < 570)
                    AccountDataGrid.Columns[i].Visibility = Visibility.Collapsed;
                else
                    AccountDataGrid.Columns[i].Visibility = Visibility.Visible;
            }
        }
    }
}

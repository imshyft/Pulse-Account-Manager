using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using Studio.Helpers;
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

        public UserProfileDataService UserProfiles { get; set; }
        public GroupSelectionService GroupSelectionService { get; set; }

        private BattleNetService _battleNetService;
        private IProfileFetchingService _profileDataFetchingService;

        private bool _mouseOverButton = false;
        private bool _isFlyoutOpen;
        private bool _isCollapsedView = false;
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
                    NavigationService?.Navigate(new AccountDetailsPage((row.DataContext as Profile)));
                }
            }
        }

        private async Task TryLaunchAccount(Profile profile, bool tryLaunchGame = false)
        {
            if (profile == null)
                return;
            if (profile.Email == null)
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
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

            _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
            {
                AllowDrop = false,
                Appearance = ControlAppearance.Success,
                Title = "Switching Account!",
                Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35),
                Opacity = 0.9
            });

            _battleNetService.OpenBattleNetWithAccount(profile.Email);
            if (!tryLaunchGame)
                return;

            bool result = await _battleNetService.WaitForMainWindow();
            if (result)
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Success,
                    Title = "Launching Game!",
                    Icon = new SymbolIcon(SymbolRegular.Checkmark12, 35),
                    Opacity = 0.9
                });

                _battleNetService.OpenBattleNet(true);
            }
            else
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    AllowDrop = false,
                    Appearance = ControlAppearance.Danger,
                    Title = "Couldn't Launch Overwatch",
                    Content = "Timed out waiting for Battle.net to load",
                    Icon = new SymbolIcon(SymbolRegular.ErrorCircle12),
                    Opacity = 0.9
                });
            }
        }

        private async void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            Profile profile = ((FrameworkElement)sender).DataContext as Profile;

            await TryLaunchAccount(profile, true);
            e.Handled = true;
        }

        private void ListButton_MouseOver(object sender, MouseEventArgs e) => _mouseOverButton = true;
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

            var result = await _profileDataFetchingService.FetchProfileAsync(profile.Battletag);
            SnackbarPresenter.AddToQue(new Snackbar(SnackbarPresenter)
            {
                Appearance = ControlAppearance.Info,
                Title = "Syncing Account",
                Content = "Please wait a moment.",
                Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
            });

            if (result.Outcome == ProfileFetchOutcome.Success)
            {
                _ = SnackbarPresenter.ImmediatelyDisplay(new Snackbar(SnackbarPresenter)
                {
                    Appearance = ControlAppearance.Success,
                    Title = "Synced Account",
                    Content = "Account profile successfully synced",
                    Icon = new SymbolIcon(SymbolRegular.ArrowClockwise16),
                });
                result.Profile.Email = profile.Email;
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
            SetDataGridNoSortingHeader(0);
            for (int i = 2; i < 5; i++)
            {
                if (width < 570)
                {
                    _isCollapsedView = true;
                    AccountDataGrid.Columns[i].Visibility = Visibility.Collapsed;
                }
                else
                {
                    _isCollapsedView = false;
                    AccountDataGrid.Columns[i].Visibility = Visibility.Visible;
                }
            }

        }

        private void AccountDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            SetDataGridNoSortingHeader(e.Column.DisplayIndex);

            // reimplement base sorting
            e.Handled = true;

            var column = e.Column;
            var direction = column.SortDirection != ListSortDirection.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            column.SortDirection = direction;

            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(AccountDataGrid.ItemsSource);
            view.CustomSort = new AccountListItemComparer(column.SortMemberPath, direction);
        }

        private void SetDataGridNoSortingHeader(int index)
        {
            if (_isCollapsedView)
                return;
            var headers = AccountDataGrid.FindChild<DataGridCellsPanel>();
            for (int i = 2; i < 5; i++)
            {
                DataGridColumnHeader header = (DataGridColumnHeader)headers.Children[i];
                SymbolIcon icon = header.FindChild<SymbolIcon>();
                if (index != i)
                {
                    icon.Visibility = Visibility.Visible;
                    icon.Symbol = SymbolRegular.LineHorizontal120;
                }
                else
                {
                    icon.Symbol = SymbolRegular.ArrowUp24;
                }

            }

        }

        private async void AccountDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject source = (DependencyObject)e.OriginalSource;

            while (source != null && !(source is DataGridRow))
                source = VisualTreeHelper.GetParent(source);

            if (source is DataGridRow row && row.DataContext is Profile profile)
            {
                await TryLaunchAccount(profile);

            }
        }

        private async void OnSwitchBnetButtonClick(object sender, RoutedEventArgs e)
        {
            Profile profile = ((FrameworkElement)sender).DataContext as Profile;

            await TryLaunchAccount(profile, false);
            e.Handled = true;
        }
    }
}

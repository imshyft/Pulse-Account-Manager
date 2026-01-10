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
using Wpf.Ui;
using Wpf.Ui.Controls;
using Button = System.Windows.Controls.Button;
using Flyout = Wpf.Ui.Controls.Flyout;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace Studio.Views
{
    // TODO: sorting broken 
    /// <summary>
    /// Interaction logic for AccountListPage.xaml
    /// </summary>
    public partial class AccountListPage : Page
    {

        public UserProfileDataService UserProfiles { get; set; }
        public GroupSelectionService GroupSelectionService { get; set; }

        private AccountActionsService _accountActionsService;
        private BattleNetService _battleNetService;
        private IProfileFetchingService _profileDataFetchingService;
        private CustomSnackbarService _snackbarService;

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
            _snackbarService = ((App)Application.Current).GetService<CustomSnackbarService>();
            _accountActionsService = ((App)Application.Current).GetService<AccountActionsService>();
            AccountDataGrid.SelectedItem = null;


        }


        private void OnUiElementPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clicks aren't detected on elements within the row, so the row must be
            // recursively searched for
            if (e.OriginalSource is DependencyObject source)
            {
                int gridRowParentSearches = 20;
                DataGridRow row = null;

                DependencyObject parent = source;
                while (gridRowParentSearches > 0)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                    if (parent is DataGridRow)
                    {
                        row = parent as DataGridRow;
                        break;
                    }
                    if (parent == null)
                        break;

                    gridRowParentSearches--;
                }
                if (row != null && !_isFlyoutOpen)
                {
                    NavigationService?.Navigate(new AccountDetailsPage(((row as DataGridRow).DataContext as ProfileV2)));
                }
            }
        }

        private async void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileV2 profile = ((FrameworkElement)sender).DataContext as ProfileV2;
            
            await _accountActionsService.TryLaunchAccount(profile, true);
            e.Handled = true;
        }

        private void OnAccountOptionsClicked(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not ProfileV2 profile)
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

        private async void OnOptionsSyncButtonClick(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not ProfileV2 profile)
                return;

            var updatedProfile = await _accountActionsService.TrySyncAccount(profile);
            if (updatedProfile != null)
            {
                UserProfiles.DeleteProfile(profile);
                UserProfiles.SaveProfile(updatedProfile);
            }

        }

        private void OnOptionsRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not ProfileV2 profile)
                return;

            UserProfiles.DeleteProfile(profile);
            _snackbarService.Show(true, s =>
            {
                s.Appearance = ControlAppearance.Success;
                s.Title = "Account deleted";
                s.Icon = new SymbolIcon(SymbolRegular.Checkmark16);
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

            if (source is DataGridRow row && row.DataContext is ProfileV2 profile)
            {
                await _accountActionsService.TryLaunchAccount(profile, true);

            }
        }

        private async void OnSwitchBnetButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileV2 profile = ((FrameworkElement)sender).DataContext as ProfileV2;

            await _accountActionsService.TryLaunchAccount(profile, false);
            e.Handled = true;
        }
    }
}

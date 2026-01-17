using MdXaml;
using Studio.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Input;

namespace Studio.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateDetailsDialog.xaml
    /// </summary>
    public partial class UpdateDetailsDialog : ContentDialog, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _updateFailed = false;
        public bool UpdateFailed
        {
            get => _updateFailed;
            set
            {
                _updateFailed = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }

        private static string repoURL = "https://github.com/imshyft/Pulse-Account-Manager/releases";

        private readonly UpdaterService _updaterService;

        public UpdateDetailsDialog(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DataContext = this;

            DialogHost = contentPresenter;

            _updaterService = ((App)Application.Current).GetService<UpdaterService>();

            IsPrimaryButtonEnabled = true;
            PrimaryButtonText = "Install Update";
            
            Loaded += OnLoaded;
        }

        public ICommand OpenGithubReleases => new RelayCommand<object>(_ =>
        {
            var startInfo = new ProcessStartInfo(repoURL)
            {
                UseShellExecute = true,
            };
            _ = Process.Start(startInfo);
        });

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string latestVersion = _updaterService.AppUpdater.LatestReleaseTagVersionStr;
            Title = "New Update Available - " + latestVersion;

            string rawMarkdown = _updaterService.AppUpdater.GetChangelog();
            if (rawMarkdown == null) return;
            Markdown md = new Markdown();
            FlowDocument doc = md.Transform(rawMarkdown);
            doc.FontFamily = Application.Current.TryFindResource("Roboto") as FontFamily;
            txtChangeLog.Document = doc;
        }

        protected override async void OnButtonClick(ContentDialogButton button)
        {
            if (button == ContentDialogButton.Primary)
            {
                IsUpdating = true;
                var downloadedAsset = await _updaterService.AppUpdater.DownloadUpdateAsync();
                IsUpdating = false;


                if (downloadedAsset == null)
                {
                    // failed
                    UpdateFailed = true;
                    IsPrimaryButtonEnabled = false;
                    return;
                }

                await _updaterService.AppUpdater.InstallUpdateAsync(downloadedAsset);
            }   

            base.OnButtonClick(button);
        }

    }
}

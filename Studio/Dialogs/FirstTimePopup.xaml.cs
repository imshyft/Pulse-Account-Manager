using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using Studio.Contracts.Services;
using Studio.Services.Files;
using Studio.Services.Storage;
using Wpf.Ui.Controls;

namespace Studio.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstTimePopup.xaml
    /// </summary>
    public partial class FirstTimePopup : ContentDialog
    {

        private PathResolverService _pathResolverService;
        private PersistAndRestoreService _storageService;
        public FirstTimePopup(ContentPresenter contentPresenter)
        {
            InitializeComponent();
            DialogHost = contentPresenter;

            _pathResolverService = ((App)Application.Current).GetService<PathResolverService>();
            _storageService = ((App)Application.Current).GetService<PersistAndRestoreService>();

            LocateOverwatchDirectory();
            LocateBattleNetConfigFile();
        }

        private void LocateOverwatchDirectory()
        {
            string path = _storageService.GetValue<string>("OverwatchDirectory");
            if (string.IsNullOrEmpty(path))
            {
                path = _pathResolverService.TryResolveOverwatchInstallation();
            }

            if (string.IsNullOrEmpty(path))
            {
                OverwatchDirInfoBar.Severity = InfoBarSeverity.Error;
                OverwatchDirInfoBar.Title = "Couldn't Locate Overwatch";
                OverwatchDirInfoBar.Message =
                    "We tried to look in common install locations but couldn't find it. Please manually locate the 'Overwatch Launcher.exe' in the install dir.";
                OverwatchDirInfoBar.IsOpen = true;
            }
            else
            {
                OverwatchDirInfoBar.Severity = InfoBarSeverity.Success;
                OverwatchDirInfoBar.Title = "Overwatch Found";
                OverwatchDirInfoBar.Message = $"Overwatch directory found at {path} !";
                OverwatchDirInfoBar.IsOpen = true;
            }


        }

        private void LocateBattleNetConfigFile()
        {
            string path = _storageService.GetValue<string>("BattleNetConfigFile");
            if (string.IsNullOrEmpty(path))
            {
                path = _pathResolverService.ResolveBattleNetConfigPath();
            }
            if (string.IsNullOrEmpty(path))
            {
                BnetConfigInfoBar.Severity = InfoBarSeverity.Error;
                BnetConfigInfoBar.Title = "Couldn't Locate Battle.Net Config";
                BnetConfigInfoBar.Message =
                    "We couldn't locate the config file. Account switching will be unable to work otherwise";
                BnetConfigInfoBar.IsOpen = true;
            }
            else
            {
                BnetConfigInfoBar.Severity = InfoBarSeverity.Success;
                BnetConfigInfoBar.Title = "Battle.Net Config Found";
                BnetConfigInfoBar.Message = $"Config File found at {path} !";
                BnetConfigTextBox.Text = path;
                BnetConfigInfoBar.IsOpen = true;
            }
        }

        private void OnOverwatchDirFilePickerButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.ExpandEnvironmentVariables("HOMEDRIVE"),
                Filter = "Overwatch Launcher |Overwatch Launcher.exe"
            };

            if (openFileDialog.ShowDialog() != true || !File.Exists(openFileDialog.FileName))
            {
                OverwatchDirInfoBar.Severity = InfoBarSeverity.Error;
                OverwatchDirInfoBar.Title = "Couldn't Locate Overwatch";
                OverwatchDirInfoBar.Message =
                    "The Overwatch launcher was not located.";
                OverwatchDirInfoBar.IsOpen = true;
            }

            OverwatchDirInfoBar.IsOpen = false;

            string directory = Directory.GetParent(openFileDialog.FileName).FullName;
            OverwatchDirTextBox.Text = directory;

            _storageService.SetValue("OverwatchDirectory", directory);
        }


    }


}

using Microsoft.Win32;
using Studio.Contracts.Services;
using Studio.Services.Files;
using Studio.Services.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Wpf.Ui.Controls;

namespace Studio.Dialogs
{
    /// <summary>
    /// Interaction logic for FirstTimePopup.xaml
    /// </summary>
    public partial class SettingsReviewDialog : ContentDialog
    {

        private readonly PathResolverService _pathResolverService;
        private readonly PersistAndRestoreService _storageService;


        public SettingsReviewDialog(ContentPresenter contentPresenter, string headerText)
        {
            InitializeComponent();
            DialogHost = contentPresenter;

            _pathResolverService = ((App)Application.Current).GetService<PathResolverService>();
            _storageService = ((App)Application.Current).GetService<PersistAndRestoreService>();

            headerTextBlock.Text = headerText;

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

            if (string.IsNullOrEmpty(path) || !File.Exists(Path.Combine(path, "Overwatch Launcher.exe")))
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

            var result = openFileDialog.ShowDialog();
            if (result != true) // cancelled
            {
                return;
            }
            else if (!File.Exists(openFileDialog.FileName))
            {
                OverwatchDirInfoBar.Severity = InfoBarSeverity.Error;
                OverwatchDirInfoBar.Title = "Couldn't Locate Overwatch";
                OverwatchDirInfoBar.Message =
                    "The Overwatch launcher was not located.";
                OverwatchDirInfoBar.IsOpen = true;
            }
            else
            {

                OverwatchDirInfoBar.IsOpen = false;

                string directory = Directory.GetParent(openFileDialog.FileName).FullName;
                OverwatchDirTextBox.Text = directory;

                _storageService.SetValue("OverwatchDirectory", directory);
            }

        }

        private void OnConfigFilePickerButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.ExpandEnvironmentVariables("HOMEDRIVE"),
                Filter = "Battle.net Config |*.config"
            };

            var result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            else if (!File.Exists(openFileDialog.FileName))
            {
                BnetConfigInfoBar.Severity = InfoBarSeverity.Error;
                BnetConfigInfoBar.Title = "Couldn't Locate Config File";
                BnetConfigInfoBar.Message =
                    "The Config File was not located.";
                BnetConfigInfoBar.IsOpen = true;
            }
            else
            {

                BnetConfigInfoBar.IsOpen = false;

                string path = openFileDialog.FileName;
                BnetConfigTextBox.Text = path;

                _storageService.SetValue("BattleNetConfigFile", path);
            }
        }
    }


}

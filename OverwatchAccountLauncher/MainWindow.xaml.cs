using OverwatchAccountLauncher.Classes;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;



namespace OverwatchAccountLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly string _filepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.OverwatchAccountLauncher";
        private static readonly string _battlenet_config = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Battle.net\\Battle.net.config";
        private static readonly string _overwatch_install_x86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Overwatch\\Overwatch Launcher.exe";
        private static readonly string _overwatch_install = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Overwatch\\Overwatch Launcher.exe";

        private UserData CurrentUser;

        public MainWindow()
        {
            InitializeComponent();
        }


        // Create New User And Save Data To File
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string name = "PlateOfSuki";
            int tag = 3588;
            UserData data = JsonHandler.CreateUserData(name, tag, "reinlover2382@gmail.com");
            if (data != null)
            {
                JsonHandler.WriteUserDataToFile(data, $"{_filepath}\\accounts\\{name}-{tag}");
            }

            name = "ReinDownMid";
            tag = 1175;
            data = JsonHandler.CreateUserData("ReinDownMid", 1175, "bowlofloki@gmail.com");
            if (data != null)
            {
                JsonHandler.WriteUserDataToFile(data, $"{_filepath}\\accounts\\{name}-{tag}");
            }
        }


        // Load ReinDownMid Data From File
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string json_string = JsonHandler.LoadJsonFromFile($"{_filepath}\\accounts\\ReinDownMid-1175");
            UserData userData = JsonHandler.DeserializeUserDataJson(json_string);
            CurrentUser = userData;
            UserAccount.Text = CurrentUser.CustomID;
        }

        // Load PlateOfSuki Data From File
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string json_string = JsonHandler.LoadJsonFromFile($"{_filepath}\\accounts\\PlateOfSuki-3588");
            UserData userData = JsonHandler.DeserializeUserDataJson(json_string);
            CurrentUser = userData;
            UserAccount.Text = CurrentUser.CustomID;
        }

        // Swap To Account
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null || CurrentUser.Email == null)
            {
                Debug.WriteLine("Select User First");
                return;
            }

            Process[] workers = Process.GetProcessesByName("Battle.net");
            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }

            string cnfg;
            using (StreamReader reader = new StreamReader(_battlenet_config))
            {
                cnfg = reader.ReadToEnd();
                cnfg = Regex.Replace(cnfg, @"\""SavedAccountNames\"": \"".*?\""", $"\"SavedAccountNames\": \"{CurrentUser.Email}\""); // bowlofloki@gmail.com bowloflokiyt@gmail.com
            }
            File.WriteAllText(_battlenet_config, cnfg);

            // "SavedAccountNames": ".*?" replace with $"\"SavedAccountNames\": \"{CurrentUser.Email}\""

            if (File.Exists(_overwatch_install))
            {
                Process.Start(_overwatch_install);
            } else if (File.Exists(_overwatch_install_x86))
            {
                Process.Start(_overwatch_install_x86);
            }
        }

        
    }
}
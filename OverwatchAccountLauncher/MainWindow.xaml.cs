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
        private AccountHandler AccountSwitcher = new AccountHandler();

        public MainWindow()
        {
            InitializeComponent();
            AccountSwitcher.LoadAccounts();
            foreach (UserData account in AccountSwitcher.UserAccounts)
            {
                // do something with them
            }
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
            ButtonOne.Content = "Load " + AccountSwitcher.UserAccounts[0].CustomID;
            ButtonTwo.Content = "Load " + AccountSwitcher.UserAccounts[1].CustomID;
        }


        // Create New User And Save Data To File
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // AccountHandler.CreateAccount("PlateOfSuki", 3588, "reinlover2382@gmail.com");
            // AccountHandler.CreateAccount("ReinDownMid", 1175, "bowlofloki@gmail.com");
            AccountHandler.CreateAccount("FstAsFoxGirl", 1143, ""); // Friend account

            AccountSwitcher.LoadAccount("ReinDownMid", 1175);
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
        }


        // Load ReinDownMid Data From File
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AccountSwitcher.LoadAccount("ReinDownMid", 1175);
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
        }

        // Load PlateOfSuki Data From File
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            AccountSwitcher.LoadAccount("PlateOfSuki", 3588);
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
        }

        // Swap To Account
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Boolean swap = AccountSwitcher.SwapToBattlenetAccount();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            AccountSwitcher.LoadAccounts();
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
        }
    }
}
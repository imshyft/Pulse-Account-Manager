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

            if (AccountSwitcher.UserAccounts.First() != null)
            {
                ButtonOne.Content = "Load " + AccountSwitcher.UserAccounts[0].CustomID;
            }
        }


        // Create New User And Save Data To File
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // AccountHandler.CreateAccount("PlateOfSuki", 3588, "");
            // AccountHandler.CreateAccount("ReinDownMid", 1175, "");
            AccountHandler.CreateAccount("FstAsFoxGirl", 1143, ""); // Friend account
            AccountHandler.CreateAccount("FstAsFrogBoi", 1143, ""); // Friend account

            if (AccountSwitcher.UserAccounts.First() != null)
            {
                ButtonOne.Content = "Load " + AccountSwitcher.UserAccounts[0].CustomID;
            }
        }


        // Switch First Account
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AccountSwitcher.SwapToBattlenetAccount(AccountSwitcher.UserAccounts.First());
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            AccountSwitcher.LoadAccounts();
            UserAccount.Text = AccountSwitcher.CurrentAccount.CustomID;
        }
    }
}
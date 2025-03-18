using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OverwatchAccountLauncher.Classes
{
    class AccountHandler
    {
        private static string _filepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.OverwatchAccountLauncher";
        private static string _battlenet_config = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Battle.net\\Battle.net.config";
        private static string _overwatch_install_x86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Overwatch\\Overwatch Launcher.exe";
        private static string _overwatch_install = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Overwatch\\Overwatch Launcher.exe";

        public List<UserData> UserAccounts = new List<UserData>();
        public List<UserData> FriendAccounts = new List<UserData>();
        public UserData? CurrentAccount;

        public AccountHandler()
        {
            if (!Directory.Exists(_filepath))
            {
                System.IO.Directory.CreateDirectory(_filepath);
                System.IO.Directory.CreateDirectory($"{_filepath}\\accounts");
            }
            if (!Directory.Exists($"{_filepath}\\accounts"))
            {
                System.IO.Directory.CreateDirectory($"{_filepath}\\accounts");
            }
        }

        public void LoadAccounts()
        {
            string[] accounts = Directory.GetFiles($"{_filepath}\\accounts\\");
            foreach (string account in accounts)
            {
                LoadAccount(account);
            }
            Debug.WriteLine(UserAccounts.Count);
            Debug.WriteLine(FriendAccounts.Count);
        }

        public void LoadAccount(string name, int tag)
        {
            LoadAccount($"{_filepath}\\accounts\\{name}-{tag}");
        }

        public void LoadAccount(string file)
        {
            string json_string = JsonHandler.LoadJsonFromFile(file);
            UserData acc = JsonHandler.DeserializeUserDataJson(json_string);
            if (acc.Email != "")
            {
                UserAccounts.Add(acc);
                SetAccount(acc);
            }
            else
            {
                FriendAccounts.Add(acc);
            }
        }

        public void SetAccount(UserData account)
        {
            CurrentAccount = account;
        }


        public Boolean SwapToBattlenetAccount(UserData account)
        {
            if (account != null)
            {
                CurrentAccount = account;
                return SwapToBattlenetAccount();
            }
            return false;
        }

        public Boolean SwapToBattlenetAccount()
        {
            if (CurrentAccount == null || CurrentAccount.Email == null)
            {
                Debug.WriteLine("Select User First");
                return false;
            }


            // Close Battle.net, allows for email to be changed in config file
            Process[] workers = Process.GetProcessesByName("Battle.net");
            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }

            // Read and replace email in config file
            string cnfg;
            using (StreamReader reader = new StreamReader(_battlenet_config))
            {
                cnfg = reader.ReadToEnd();
                cnfg = Regex.Replace(cnfg, @"\""SavedAccountNames\"": \"".*?\""", $"\"SavedAccountNames\": \"{CurrentAccount.Email}\"");
            }
            File.WriteAllText(_battlenet_config, cnfg);

            // Launch The Overwatch Executable to open Battle.net
            if (File.Exists(_overwatch_install))
            {
                Process.Start(_overwatch_install);
                return true;
            }
            else if (File.Exists(_overwatch_install_x86))
            {
                Process.Start(_overwatch_install_x86);
                return true;
            }
            return false;
        }

        public static void CreateAccount(string name, int tag, string? email)
        {
            UserData data = JsonHandler.CreateUserData(name, tag, email);
            if (data != null)
            {
                Debug.WriteLine($"{_filepath}\\accounts\\{name}-{tag}");
                JsonHandler.WriteUserDataToFile(data, $"{_filepath}\\accounts\\{name}-{tag}");
            }
        }

    }
}

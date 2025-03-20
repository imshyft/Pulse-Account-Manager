using Studio.Services.Storage;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Studio.Services
{
    // TODO : Create methods to set the email, close, and launch battle.net
    public class BattleNetService
    {
        private readonly PersistAndRestoreService _persistAndRestoreService;
        private readonly string _battleNetConfigPath;
        private readonly string _overwatchLauncherPath;

        public BattleNetService()
        {
            _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();
            _battleNetConfigPath = _persistAndRestoreService.GetValue<string>("BattleNetConfigPath");

            string overwatchDirectory = _persistAndRestoreService.GetValue<string>("OverwatchDirectory");
            _overwatchLauncherPath = Path.Combine(overwatchDirectory, "Overwatch Launcher.exe");
        }

        public void CloseBattleNet()
        {
            Process[] workers = Process.GetProcessesByName("Battle.net");
            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }
        }

        public void OpenBattleNet()
        {
            Process.Start(_overwatchLauncherPath);
        }

        public void OpenBattleNetWithAccount(string email)
        {
            CloseBattleNet();
            SetBattleNetAccount(email);
            OpenBattleNet();
        }

        public void OpenBattleNetWithEmptyAccount()
        {
            CloseBattleNet();
            ResetBattleNetAccount();
            OpenBattleNet();
        }

        public void SetBattleNetAccount(string email)
        {
            string config;
            using (StreamReader reader = new StreamReader(_battleNetConfigPath))
            {
                config = reader.ReadToEnd();
                config = Regex.Replace(config, @"\""SavedAccountNames\"": \"".*?\""", $"\"SavedAccountNames\": \"{email}\"");
            }
            File.WriteAllText(_battleNetConfigPath, config);
        }

        public void ResetBattleNetAccount()
        {
            SetBattleNetAccount("");
        }
        public string GetBattleNetAccount()
        {
            string config;
            using (StreamReader reader = new StreamReader(_battleNetConfigPath))
            {
                config = reader.ReadToEnd();
                Match match = Regex.Match(config, @"\""SavedAccountNames\"": \""(.*?)\""");
                if (!match.Success)
                    return "";

                string email = match.Groups[1].Value;
                return email;
            }

        }
    }
}

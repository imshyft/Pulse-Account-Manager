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
        private readonly ConfigStorageService _configStorageService;
        private readonly string _battleNetConfigPath;
        private readonly string _overwatchLauncherPath;

        public BattleNetService()
        {
            _configStorageService = ((App)Application.Current).GetService<ConfigStorageService>();
            _battleNetConfigPath = _configStorageService.GetValue<string>("BattleNetConfigPath");

            string overwatchDirectory = _configStorageService.GetValue<string>("OverwatchDirectory");
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

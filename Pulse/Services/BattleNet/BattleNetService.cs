using Studio.Services.Storage;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using Studio.Models;
using Studio.Services.BattleNet;
using System;
using System.Windows.Media.Animation;
using AngleSharp.Text;
using System.Collections.Specialized;
using System.Windows.Controls.Ribbon;
using Studio.Contracts.Services;

namespace Studio.Services
{
    public class BattleNetService
    {
        private readonly PersistAndRestoreService _persistAndRestoreService;
        private string _battleNetConfigPath;
        private string _overwatchLauncherPath;
        private readonly IMemoryReaderService _memoryReaderService;
        private bool _isReady;

        public BattleNetService(IMemoryReaderService memoryReaderService)
        {
            _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();
            _memoryReaderService = memoryReaderService;

            _isReady = IsPathsValid();


        }

        public bool IsPathsValid()
        {
            _battleNetConfigPath = _persistAndRestoreService.GetValue<string>("BattleNetConfigPath");
            if (string.IsNullOrEmpty(_battleNetConfigPath))
                return false;
            string overwatchDirectory = _persistAndRestoreService.GetValue<string>("OverwatchDirectory");
            if (string.IsNullOrEmpty(overwatchDirectory))
                return false;

            _overwatchLauncherPath = Path.Combine(overwatchDirectory, "Overwatch Launcher.exe");
            if (!Directory.Exists(_overwatchLauncherPath))
                return false;

            return true;
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

        public void OpenBattleNet(bool launchGame = false)
        {
            Process.Start(_overwatchLauncherPath, launchGame ? "--exec=\"launch Pro\"" : "");
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

        public async Task<bool> WaitForMainWindow()
        {
            var cancel = new CancellationTokenSource(80000);
            try
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        cancel.Token.ThrowIfCancellationRequested();

                        var procs = Process.GetProcessesByName("Battle.net");
                        Debug.WriteLine(procs.Select(x => x.MainWindowTitle));
                        if (procs.Length > 0 && procs.Any(x => x.MainWindowTitle == "Battle.net"))
                            return;

                        await Task.Delay(500, cancel.Token);
                    }
                }); ;
                await Task.Delay(3000);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
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

        public BattleTagV2 ReadBattleTagFromMemory()
        {
            Process[] processes = Process.GetProcessesByName("Battle.net");
            Process process;
            if (processes.Length == 0)
            {
                process = Process.Start(_overwatchLauncherPath);
            }
            else
            {
                process = processes[0];
            }

            //var friends = _memoryReaderService.FindBlizzardFriends(process.Id);


            //var tag = BattleNetMemoryReaderService_OLD.GetUserBattleTagString(process.Handle);
            var tag = _memoryReaderService.GetUserBattleTagString(process.Handle);

            if (tag == "")
                return null;

            BattleTagV2 battleTag = new BattleTagV2(tag);

            return battleTag;
        }

        public BattleTagV2[] ReadFriendsListFromMemory()
        {
            Process[] processes = Process.GetProcessesByName("Battle.net");
            Process process;
            if (processes.Length == 0)
            {
                process = Process.Start(_overwatchLauncherPath);
            }
            else
            {
                process = processes[0];
            }
            var tags = _memoryReaderService.GetFriendBattleTagStrings(process.Handle);
            BattleTagV2[] battleTags = tags.Select(x => new BattleTagV2(x)).ToArray();

            return battleTags;

        }
    }
}

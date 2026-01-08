using AngleSharp.Text;
using HarfBuzzSharp;
using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.BattleNet;
using Studio.Services.Storage;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace Studio.Services
{
    public class BattleNetService
    {
        private readonly PersistAndRestoreService _persistAndRestoreService;
        private string _battleNetConfigPath;
        private string _overwatchLauncherPath;
        private readonly IMemoryReaderService _memoryReaderService;

        private List<string> _storedFriendBattleTags = [];
        private List<string> _storedUserBattleTags = [];

        // only re-scan memory if its a new instance of battle.net
        private int _lastFriendScanPID = 0;
        private int _lastUserScanPID = 0;

        #region IMPORTS
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(
        uint dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);

        const uint PROCESS_VM_READ = 0x0010;
        const uint PROCESS_QUERY_INFORMATION = 0x0400;
        #endregion

        public BattleNetService(IMemoryReaderService memoryReaderService)
        {
            _persistAndRestoreService = ((App)Application.Current).GetService<PersistAndRestoreService>();
            _memoryReaderService = memoryReaderService;

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
            if (!File.Exists(_overwatchLauncherPath))
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

        public async Task<BattleTagV2[]> ReadBattleTagsFromMemory(CancellationToken? token = null)
        {
            Process process = getBattleNetProcess();
            IntPtr handle = GetHandle(process.Id);

            string[] tags;
            if (_storedUserBattleTags.Count == 0 || _lastUserScanPID != process.Id)
            {
                _lastUserScanPID = process.Id;
                tags = await _memoryReaderService.GetUserBattletagStrings(handle, token ?? CancellationToken.None);
            }
            else
            {
                tags = _storedUserBattleTags.ToArray();
            }

            BattleTagV2[] battleTags = tags.Select(x => new BattleTagV2(x)).ToArray();

            return battleTags;
        }

        public async Task<BattleTagV2[]> ReadFriendsListFromMemory(CancellationToken? token = null)
        {
            Process process = getBattleNetProcess();
            IntPtr handle = GetHandle(process.Id);

            string[] tags;
            if (_storedFriendBattleTags.Count == 0 || _lastFriendScanPID != process.Id)
            {
                _lastFriendScanPID = process.Id;
                tags = await _memoryReaderService.GetFriendBattletagStrings(handle, token ?? CancellationToken.None);
            }
            else
            {
                tags = _storedFriendBattleTags.ToArray();
            }

            BattleTagV2[] battleTags = tags.Select(x => new BattleTagV2(x)).ToArray();

            return battleTags;

        }

        private Process getBattleNetProcess()
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

            return process;
        }

        private IntPtr GetHandle(int pid)
        {
            IntPtr hProc = OpenProcess(
            PROCESS_VM_READ | PROCESS_QUERY_INFORMATION,
            false,
            pid);

            if (hProc == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return hProc;
        }
    }
}

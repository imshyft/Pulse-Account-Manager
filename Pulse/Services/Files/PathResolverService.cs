using System.IO;
using Studio.Contracts.Services;
using Studio.Services.Storage;

namespace Studio.Services.Files
{
    public class PathResolverService
    {
        private readonly PersistAndRestoreService _persistAndRestoreService;


        public PathResolverService(PersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
        }

        public string TryResolveOverwatchInstallation()
        {
            string x64Folder = Environment.GetEnvironmentVariable("programfiles(x86)");
            string x64InstallLocation = Path.Combine(x64Folder, "Overwatch");
            if (Directory.Exists(x64InstallLocation))
            {
                _persistAndRestoreService.SetValue("OverwatchDirectory", x64InstallLocation);
                _persistAndRestoreService.PersistData();
                return x64InstallLocation;
            }

            return "";
        }

        public string ResolveBattleNetConfigPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string appDataBnetConfigFile = Path.Combine(appData, "Battle.net", "Battle.net.config");

            if (!File.Exists(appDataBnetConfigFile))
                return "";

            _persistAndRestoreService.SetValue("BattleNetConfigPath", appDataBnetConfigFile);
            _persistAndRestoreService.PersistData();
            return appDataBnetConfigFile;
        }
    }
}

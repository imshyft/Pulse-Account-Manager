using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updatum;

namespace Studio.Services.ApplicationServices
{
    public class UpdaterService
    {
        private const string RepositoryOwner = "pressctrlkey";
        private const string RepositoryName = "launcher-updater-test";

        private readonly UpdatumManager _appUpdater = new(RepositoryOwner, RepositoryName)
        {
            InstallUpdateWindowsExeType = UpdatumWindowsExeType.Installer,
            InstallUpdateWindowsInstallerArguments = "/qb",
            
        };

        public UpdatumManager AppUpdater => _appUpdater;



    }
}

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
        private const string RepositoryOwner = "imshyft";
        private const string RepositoryName = "Pulse-Account-Manager";

        private readonly UpdatumManager _appUpdater = new(RepositoryOwner, RepositoryName)
        {
            InstallUpdateWindowsExeType = UpdatumWindowsExeType.Installer,
            InstallUpdateWindowsInstallerArguments = "/qb",
            
        };

        public UpdatumManager AppUpdater => _appUpdater;



    }
}

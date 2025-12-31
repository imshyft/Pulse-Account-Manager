using Microsoft.Extensions.Options;
using Studio.Contracts.Services;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Services.Data.AppPaths
{
    class NewUserAppPaths : IAppPaths
    {
        public string Root { get; }

        public string Config => Path.Combine(Root, "config.json");

        public string Profiles => Path.Combine(Root, "User");

        public string Favourites => Path.Combine(Root, "Favourites");

        public NewUserAppPaths(IOptions<AppConfig> appConfig)
        {
            Root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                appConfig.Value.ConfigurationFolderName,
                ".debug");
            
            // wipe debug directory on launch to simulate new user
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, true);
            }
            Directory.CreateDirectory(Root);
        }
    }
}

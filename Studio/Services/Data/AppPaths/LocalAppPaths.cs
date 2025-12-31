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
    class LocalAppPaths : IAppPaths
    {
        public string Root { get; }

        public string Config => Path.Combine(Root, "config.json");

        public string Profiles => Path.Combine(Root, "User");

        public string Favourites => Path.Combine(Root, "Favourites");

        public LocalAppPaths(IOptions<AppConfig> appConfig)
        {
            Root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appConfig.Value.ConfigurationFolderName);

            Directory.CreateDirectory(Root);
        }
    }
}

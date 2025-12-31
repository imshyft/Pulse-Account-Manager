using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Contracts.Services
{
    public interface IAppPaths
    {
        string Root { get; }
        string Config { get; }
        string Profiles { get; }
        string Favourites { get; }
    }

}

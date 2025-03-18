using System;
using System.Collections.Generic;
using System.Text;
using Studio.Core.Models;

namespace Studio.Core.Contracts.Services
{
    public interface ISampleDataService
    {
        IEnumerable<ProfileData> GetFavouriteProfiles();
        IEnumerable<ProfileData> GetUserProfiles();
    }
}

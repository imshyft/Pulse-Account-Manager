using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Studio.Contracts.Services;
using Studio.Models;

namespace Studio.Services.Data
{
    public class DesignTimeSampleData
    {
        public ObservableCollection<ProfileV2> FilteredProfiles { get; set; } = [];
        public UserProfileDataService UserProfiles { get; set; }
        public ProfileV2 Profile { get; set; }
        public DesignTimeSampleData()
        {
            UserProfiles = new SampleUserProfileDataService();
            UserProfiles.LoadProfilesFromDisk();

            var sampleFavData = new SampleFavouriteProfileDataService();
            sampleFavData.LoadProfilesFromDisk();

            foreach (var data in sampleFavData.Profiles)
            {
                FilteredProfiles.Add(data);
            }

            Profile = UserProfiles.Profiles[0];
        }
    }
}

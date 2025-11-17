using Microsoft.Extensions.Options;
using Studio.Models;

//using Studio.Models;
using Studio.Services.Files;
using System.Collections.ObjectModel;
using System.IO;

namespace Studio.Contracts.Services
{
    public abstract class ProfileDataService
    {
        public string ProfileDirectory { get; protected set; }

        public ObservableCollection<ProfileV2> Profiles { get; set; } = [];


        public virtual void SaveProfile(ProfileV2 profile)
        {
            ProfileV2 duplicate = FindDuplicateProfile(profile);
            if (duplicate != null)
            {
                Profiles.Remove(duplicate);
            }

            Profiles.Add(profile);

        }

        public virtual void DeleteProfile(ProfileV2 profile)
        {
            Profiles.Remove(profile);
        }

        public virtual bool ContainsProfile(ProfileV2 profile)
        {
            return FindDuplicateProfile(profile) != null;
        }

        private ProfileV2 FindDuplicateProfile(ProfileV2 profile)
        {
            foreach (var item in Profiles)
            {
                if (item.Battletag.ToString() == profile.Battletag.ToString()) return item;
            }

            return null;
        }

        public abstract ProfileV2 ReadProfile(BattleTagV2 battletag);
        public abstract void LoadProfilesFromDisk();
    }

    public abstract class FavouriteProfileDataService : ProfileDataService { }
    public abstract class UserProfileDataService : ProfileDataService { }
}

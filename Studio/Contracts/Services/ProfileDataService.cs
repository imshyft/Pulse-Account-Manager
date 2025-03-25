using Microsoft.Extensions.Options;
using Studio.Models;
using Studio.Services.Files;
using System.Collections.ObjectModel;
using System.IO;

namespace Studio.Contracts.Services
{
    public abstract class ProfileDataService
    {
        public string ProfileDirectory { get; protected set; }

        public ObservableCollection<Profile> Profiles { get; set; } = new ObservableCollection<Profile>();


        public virtual void SaveProfile(Profile profile)
        {
            Profile duplicate = FindDuplicateProfile(profile);
            if (duplicate != null)
            {
                DeleteProfile(duplicate);
            }

            Profiles.Add(profile);

        }

        public virtual void DeleteProfile(Profile profile)
        {
            Profiles.Remove(profile);
        }

        public virtual bool ContainsProfile(Profile profile)
        {
            return FindDuplicateProfile(profile) != null;
        }

        private Profile FindDuplicateProfile(Profile profile)
        {
            foreach (var item in Profiles)
            {
                if (item.Battletag.ToString() == profile.Battletag.ToString()) return item;
            }

            return null;
        }

        public abstract Profile ReadProfile(BattleTag battletag);
        public abstract void LoadProfilesFromDisk();
    }

    public abstract class FavouriteProfileDataService : ProfileDataService { }
    public abstract class UserProfileDataService : ProfileDataService { }
}

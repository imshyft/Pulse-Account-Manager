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

        public ObservableCollection<UserData> Profiles { get; set; } = new ObservableCollection<UserData>();


        public virtual void SaveProfile(UserData profile)
        {
            Profiles.Add(profile);
        }

        public virtual void DeleteProfile(UserData profile)
        {
            Profiles.Remove(profile);
        }

        public abstract UserData ReadProfile(BattleTag battletag);
        public abstract void LoadProfilesFromDisk();
    }

    public abstract class FavouriteProfileDataService : ProfileDataService { }
    public abstract class UserProfileDataService : ProfileDataService { }
}

using System.Collections.ObjectModel;
using System.Diagnostics;
using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;

//using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Data
{
    public class SampleFavouriteProfileDataService : FavouriteProfileDataService
    {

        private readonly Random _rnd = new(1);

        private List<ProfileV2> CreateProfiles(int count)
        {
            int snapshotsCount = 10;
            List<ProfileV2> data = new List<ProfileV2>();
            for (int i = 0; i < count; i++)
            {
                bool missingDetails = _rnd.NextDouble() < 0.2;
                data.Add(new ProfileV2()
                {
                    Battletag = new BattleTagV2("Username", _rnd.Next(1000, 9999).ToString()),
                    AvatarURL = missingDetails ?
                        null : $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png",
                    CustomName = $"Name{_rnd.Next(1000)}",
                    Email = "email@123",

                    Snapshots = SampleAccountCreationHelper.GenerateRandomSnapshots(_rnd, snapshotsCount)
                });
            }

            return data;
        }

        public override void SaveProfile(ProfileV2 profile)
        {
            Debug.WriteLine("Save Favourite Profile Method Called.");
            base.SaveProfile(profile);
        }

        public override ProfileV2 ReadProfile(BattleTagV2 battletag)
        {
            return CreateProfiles(1)[0];
        }

        public override void DeleteProfile(ProfileV2 profile)
        {
            Debug.WriteLine("Delete Favourite Profile Method Called.");
            base.DeleteProfile(profile);
        }

        public override void LoadProfilesFromDisk()
        {
            foreach (var profile in CreateProfiles(10))
            {
                Profiles.Add(profile);
            }
        }

    }
}

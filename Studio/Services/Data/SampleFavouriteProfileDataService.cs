using System.Collections.ObjectModel;
using System.Diagnostics;
using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Data
{
    public class SampleFavouriteProfileDataService : FavouriteProfileDataService
    {

        private readonly Random _rnd = new(1);


        private List<RankMoment> RandomRankMoments(int count)
        {
            return Enumerable.Range(0, count)
                .Select(j =>
                    new RankMoment()
                    {
                        Date = 1726641630 + j * _rnd.Next(80000, 89000),
                        Rank = Rank.RankFromSr(_rnd.Next(500, 5000))
                    })
                .ToList();
        }

        private List<UserData> CreateProfiles(int count)
        {
            List<UserData> data = new List<UserData>();
            for (int i = 0; i < count; i++)
            {
                data.Add(new UserData()
                {
                    Battletag = new Battletag("Username", _rnd.Next(1000, 9999).ToString()),

                    Avatar = $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png",
                    CustomId = $"Name{_rnd.Next(1000)}",
                    Email = null,
                    LastUpdate = _rnd.Next(1, 1000000),
                    RankedCareer = new RankedCareer()
                    {
                        Damage = new Damage()
                        {
                            CurrentRank = Rank.RankFromSr(_rnd.Next(500, 5000)),
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(10)
                        },

                        Support = new Support()
                        {
                            CurrentRank = Rank.RankFromSr(_rnd.Next(500, 5000)),
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(10)
                        },

                        Tank = new Tank()
                        {
                            CurrentRank = Rank.RankFromSr(_rnd.Next(500, 5000)),
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(10)
                        },
                    },
                    TimesLaunched = _rnd.Next(3, 80),
                    TimesSwitched = _rnd.Next(10, 300)

                });
            }

            return data;
        }

        public override void SaveProfile(UserData profile)
        {
            Debug.WriteLine("Save Favourite Profile Method Called.");
            base.SaveProfile(profile);
        }

        public override UserData ReadProfile(Battletag battletag)
        {
            return CreateProfiles(1)[0];
        }

        public override void DeleteProfile(UserData profile)
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

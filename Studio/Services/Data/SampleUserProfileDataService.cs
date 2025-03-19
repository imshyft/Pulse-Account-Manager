using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Studio.Contracts.Services;
using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Data
{
    public class SampleUserProfileDataService : UserProfileDataService
    {

        private readonly Random _rnd = new(1);
        private int[] _randomDates;

        private List<RankMoment> RandomRankMoments(int count)
        {
            return Enumerable.Range(0, count)
                .Select(j =>
                    new RankMoment()
                    {
                        Date = _randomDates[j],
                        Rank = Rank.RankFromSR(_rnd.Next(500, 5000))
                    })
                .ToList();
        }

        private List<UserData> CreateProfiles(int count)
        {
            int rankMoments = 10;
            List<UserData> data = new List<UserData>();
            for (int i = 0; i < count; i++)
            {
                _randomDates = new int[rankMoments];
                int date = 1726641630;
                for (int j = 0; j < rankMoments; j++)
                {
                    _randomDates[j] = date;
                    date += _rnd.Next(80000, 160000);
                }

                data.Add(new UserData()
                {
                    Battletag = new Battletag("Username", _rnd.Next(1000, 9999).ToString()),
                    //Username = "Username",
                    //Tag = _rnd.Next(1000, 9999).ToString(),
                    Avatar = _rnd.NextDouble() > 0.3 ? 
                        $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png" : null,
                    CustomId = "Name",
                    Email = "email@123.com",
                    LastUpdate = _rnd.Next(1, 1000000),
                    RankedCareer = new RankedCareer()
                    {
                        Damage = new Damage()
                        {
                            CurrentRank = _rnd.NextDouble() > 0.4 ? Rank.RankFromSR(_rnd.Next(500, 5000)) : null,
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(rankMoments)
                        },

                        Support = new Support()
                        {
                            CurrentRank = _rnd.NextDouble() > 0.4 ? Rank.RankFromSR(_rnd.Next(500, 5000)) : null,
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(rankMoments)
                        },

                        Tank = new Tank()
                        {
                            CurrentRank = _rnd.NextDouble() > 0.4 ? Rank.RankFromSR(_rnd.Next(500, 5000)) : null,
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = RandomRankMoments(rankMoments)
                        },
                    },
                    TimesLaunched = _rnd.Next(3, 80),
                    TimesSwitched = _rnd.Next(10, 300)


                    //Account = new Account()
                    //{
                    //    Id = $"Test#{_rnd.Next(1000, 9999)}",
                    //    Name = $"User{_rnd.Next(10,60)}",
                    //    SymbolCode = _rnd.Next(5760, 5790)
                    //},
                    //AvatarId = "daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c",
                    //DamageRankHistory = Enumerable
                    //    .Repeat(0, 10)
                    //    .Select(x => _rnd.Next(2000, 4000))
                    //    .ToArray(),
                    //TankRankHistory = Enumerable
                    //    .Repeat(0, 10)
                    //    .Select(x => _rnd.Next(2000, 4000))
                    //    .ToArray(),
                    //SupportRankHistory = Enumerable
                    //    .Repeat(0, 10)
                    //    .Select(x => _rnd.Next(2000, 4000))
                    //    .ToArray(),
                    //TankRankActive = _rnd.Next(2000, 4000),
                    //SupportRankActive = _rnd.Next(2000, 4000),
                    //DamageRankActive = _rnd.Next(2000, 4000),

                });
            }

            return data;
        }

        public override void SaveProfile(UserData profile)
        {
            Debug.WriteLine("Save User Profile Method Called.");
            base.SaveProfile(profile);
        }

        public override UserData ReadProfile(Battletag battletag)
        {
            return CreateProfiles(1)[0];
        }

        public override void DeleteProfile(UserData profile)
        {
            Debug.WriteLine("Delete User Profile Method Called.");
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

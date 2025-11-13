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

        private readonly Random _rnd = new();
        private int[] _randomDates;

        private List<RankMoment> RandomRankMoments(int count)
        {
            List<int> start = [_rnd.Next(500, 4500)];
            for (int i = 1; i < count; ++i)
            {
                start.Add(start[i - 1] + _rnd.Next(-300, 300));
            }
            return start.Select((r, i) => new RankMoment()
            {
                Date = _randomDates[i],
                Rank = Rank.RankFromSr(r)
            }).ToList();
        }

        private List<Profile> CreateProfiles(int count)
        {
            int rankMoments = 10;
            List<Profile> data = new List<Profile>();
            for (int i = 0; i < count; i++)
            {
                _randomDates = new int[rankMoments];
                int date = 1726641630;
                for (int j = 0; j < rankMoments; j++)
                {
                    _randomDates[j] = date;
                    date += _rnd.Next(80000, 160000);
                }

                bool missingDetails = _rnd.NextDouble() < 0.3;

                var tankMoments = RandomRankMoments(rankMoments);
                var dmgMoments = RandomRankMoments(rankMoments);
                var suppMoments = RandomRankMoments(rankMoments);

                data.Add(new Profile()
                {
                    Battletag = new BattleTag("Username", _rnd.Next(1000, 9999).ToString()),
                    //Username = "Username",
                    //Tag = _rnd.Next(1000, 9999).ToString(),
                    Avatar = missingDetails ?
                        null : $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png",
                    CustomId = $"Name{_rnd.Next(1000)}",
                    Email = "email@123",
                    LastUpdate = _rnd.Next(1, 1000000),

                    

                    RankedCareer = new RankedCareer()
                    {
                        Damage = _rnd.NextDouble() > 0.4 ? new Damage()
                        {
                            RankMoments = dmgMoments,

                            CurrentRank = dmgMoments.Last().Rank,
                            PeakRank = RandomRankMoments(1)[0],
                        } : null,
                        Support = _rnd.NextDouble() > 0.4 ? new Support()
                        {
                            CurrentRank = suppMoments.Last().Rank,
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = suppMoments
                        } : null,

                        Tank = _rnd.NextDouble() > 0.4 ? new Tank()
                        {
                            CurrentRank = tankMoments.Last().Rank,
                            PeakRank = RandomRankMoments(1)[0],
                            RankMoments = tankMoments
                        } : null,
                    },
                    TimesLaunched = _rnd.Next(3, 80),
                    TimesSwitched = _rnd.Next(10, 300)

                });
            }

            return data;
        }

        public override void SaveProfile(Profile profile)
        {
            Debug.WriteLine("Save User Profile Method Called.");
            base.SaveProfile(profile);
        }

        public override Profile ReadProfile(BattleTag battletag)
        {
            return CreateProfiles(1)[0];
        }

        public override void DeleteProfile(Profile profile)
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

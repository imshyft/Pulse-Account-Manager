using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Studio.Contracts.Services;
using Studio.Models;

//using Studio.Models;
using Studio.Services.Files;

namespace Studio.Services.Data
{
    public class SampleUserProfileDataService : UserProfileDataService
    {

        private readonly Random _rnd = new();
        private int[] _randomDates;

        //private List<RankMoment> RandomRankMoments(int count)
        //{
        //    List<int> start = [_rnd.Next(500, 4500)];
        //    for (int i = 1; i < count; ++i)
        //    {
        //        start.Add(start[i - 1] + _rnd.Next(-300, 300));
        //    }
        //    return start.Select((r, i) => new RankMoment()
        //    {
        //        Date = _randomDates[i],
        //        Rank = Rank.RankFromSr(r)
        //    }).ToList();
        //}

        private Dictionary<StatisticType, float> GenerateRandomStats()
        {
            var dict = new Dictionary<StatisticType, float>();
            dict[StatisticType.Damage] = _rnd.Next(3000, 12000);
            dict[StatisticType.Healing] = _rnd.Next(3000, 12000);
            dict[StatisticType.TimePlayed] = _rnd.Next(300, 900);
            dict[StatisticType.Elims] = _rnd.Next(5, 40);
            dict[StatisticType.Deaths] = _rnd.Next(2, 12);
            dict[StatisticType.Winrate] = (float)_rnd.NextDouble();

            return dict;

        }

        private List<ProfileSnapshotV2> GenerateRandomSnapshots(int count)
        {
            List<ProfileSnapshotV2> snapshots = [];
            int tankRank = _rnd.Next(500, 4500);
            int dmgRank = _rnd.Next(500, 4500);
            int suppRank = _rnd.Next(500, 4500);
            //_randomDates = new int[count];
            int date = 1726641630;
            for (int j = 0; j < count; j++)
            {
                //_randomDates[j] = date;
                bool noTank = _rnd.NextDouble() < 0.3;
                bool noDmg = _rnd.NextDouble() < 0.3;
                bool noSupp = _rnd.NextDouble() < 0.3;

                Tank tank = new()
                {
                    Rank = noTank ? null : RankV2.RankFromSr(tankRank += _rnd.Next(-2, 2) * 100),
                    Stats = noTank ? null : GenerateRandomStats()
                };

                Support supp = new()
                {
                    Rank = noSupp ? null : RankV2.RankFromSr(suppRank += _rnd.Next(-2, 2) * 100),
                    Stats = noSupp ? null : GenerateRandomStats()
                };

                Damage dmg = new()
                {
                    Rank = noDmg ? null : RankV2.RankFromSr(suppRank += _rnd.Next(-2, 2) * 100),
                    Stats = noDmg ? null : GenerateRandomStats()
                };

                snapshots.Add(new ProfileSnapshotV2()
                {
                    Timestamp = date,
                    Tank = tank,
                    Damage = dmg,
                    Support = supp
                });

                date += _rnd.Next(80000, 160000);
            }

            return snapshots;
        }

        private List<ProfileV2> CreateProfiles(int count)
        {
            int snapshotsCount = 10;
            List<ProfileV2> data = new List<ProfileV2>();
            for (int i = 0; i < count; i++)
            {


                bool missingDetails = _rnd.NextDouble() < 0.2;

                //var tankMoments = RandomRankMoments(rankMoments);
                //var dmgMoments = RandomRankMoments(rankMoments);
                //var suppMoments = RandomRankMoments(rankMoments);

                data.Add(new ProfileV2()
                {
                    Battletag = new BattleTagV2("Username", _rnd.Next(1000, 9999).ToString()),
                    //Username = "Username",
                    //Tag = _rnd.Next(1000, 9999).ToString(),
                    AvatarURL = missingDetails ?
                        null : $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png",
                    CustomName = $"Name{_rnd.Next(1000)}",
                    Email = "email@123",

                    Snapshots = GenerateRandomSnapshots(snapshotsCount)
                    //RankedCareer = new RankedCareer()
                    //{
                    //    Damage = _rnd.NextDouble() > 0.4 ? new Damage()
                    //    {
                    //        RankMoments = dmgMoments,

                    //        CurrentRank = dmgMoments.Last().Rank,
                    //        PeakRank = RandomRankMoments(1)[0],
                    //    } : null,
                    //    Support = _rnd.NextDouble() > 0.4 ? new Support()
                    //    {
                    //        CurrentRank = suppMoments.Last().Rank,
                    //        PeakRank = RandomRankMoments(1)[0],
                    //        RankMoments = suppMoments
                    //    } : null,

                    //    Tank = _rnd.NextDouble() > 0.4 ? new Tank()
                    //    {
                    //        CurrentRank = tankMoments.Last().Rank,
                    //        PeakRank = RandomRankMoments(1)[0],
                    //        RankMoments = tankMoments
                    //    } : null,
                    //},
                    //TimesLaunched = _rnd.Next(3, 80),
                    //TimesSwitched = _rnd.Next(10, 300)

                });
            }

            return data;
        }

        public override void SaveProfile(ProfileV2 profile)
        {
            Debug.WriteLine("Save User Profile Method Called.");
            base.SaveProfile(profile);
        }

        public override ProfileV2 ReadProfile(BattleTagV2 battletag)
        {
            return CreateProfiles(1)[0];
        }

        public override void DeleteProfile(ProfileV2 profile)
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

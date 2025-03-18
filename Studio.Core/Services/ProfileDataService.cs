using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using Studio.Core.Contracts.Services;
using Studio.Core.Models;

namespace Studio.Core.Services
{
    public class ProfileDataService : IProfileDataService
    {
        private readonly Random _rnd = new(1);


        private List<RankMoment> randomRankMoments(int count)
        {
            return Enumerable.Range(0, count)
                .Select(j =>
                    new RankMoment()
                    {
                        Date = 1726641630 + j * _rnd.Next(80000, 89000),
                        Rank = Rank.RankFromSR(_rnd.Next(500, 5000))
                    })
                .ToList();
        }

        public IEnumerable<UserData> GetFavouriteProfiles()
        {
            List<UserData> data = new List<UserData>();
            for (int i = 0; i < 10; i++)
            {
                data.Add(new UserData()
                {
                    Username = "Username",
                    Avatar = $"https://d15f34w2p8l1cc.cloudfront.net/overwatch/daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c.png",
                    CustomID = "Name",
                    Email = "email@123.com",
                    Last_update = _rnd.Next(1, 1000000),
                    RankedCareer = new RankedCareer()
                    {
                        Damage = new Damage()
                        {
                            CurrentRank = Rank.RankFromSR(_rnd.Next(500, 5000)),
                            PeakRank = randomRankMoments(1)[0],
                            RankMoments = randomRankMoments(10)
                        },

                        Support = new Support()
                        {
                            CurrentRank = Rank.RankFromSR(_rnd.Next(500, 5000)),
                            PeakRank = randomRankMoments(1)[0],
                            RankMoments = randomRankMoments(10)
                        },

                        Tank = new Tank()
                        {
                            CurrentRank = Rank.RankFromSR(_rnd.Next(500, 5000)),
                            PeakRank = randomRankMoments(1)[0],
                            RankMoments = randomRankMoments(10)
                        },
                    },
                    Tag = _rnd.Next(1000, 9999).ToString(),
                    Rank_history = null,
                    Times_launched = _rnd.Next(3, 80),
                    Times_switched = _rnd.Next(10, 300)
                    

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

        public IEnumerable<UserData> GetUserProfiles()
        {
            return GetFavouriteProfiles();
        }
    }
}

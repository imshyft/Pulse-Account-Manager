using System;
using System.Collections.Generic;
using System.Text;
using Studio.Core.Contracts.Services;
using Studio.Core.Models;

namespace Studio.Core.Services
{
    public class SampleDataService : ISampleDataService
    {
        private readonly Random _rnd = new(1);

        public IEnumerable<ProfileData> GetFavouriteProfiles()
        {
            List<ProfileData> data = new List<ProfileData>();
            for (int i = 0; i < 10; i++)
            {
                data.Add(new ProfileData()
                {
                    Account = new Account()
                    {
                        Id = $"Test#{_rnd.Next(1000, 9999)}",
                        Name = $"User{_rnd.Next(10,60)}",
                        SymbolCode = _rnd.Next(5760, 5790)
                    },
                    AvatarId = "daeddd96e58a2150afa6ffc3c5503ae7f96afc2e22899210d444f45dee508c6c",
                    DamageRankHistory = Enumerable
                        .Repeat(0, 10)
                        .Select(x => _rnd.Next(2000, 4000))
                        .ToArray(),
                    TankRankHistory = Enumerable
                        .Repeat(0, 10)
                        .Select(x => _rnd.Next(2000, 4000))
                        .ToArray(),
                    SupportRankHistory = Enumerable
                        .Repeat(0, 10)
                        .Select(x => _rnd.Next(2000, 4000))
                        .ToArray(),
                    TankRankActive = _rnd.Next(2000, 4000),
                    SupportRankActive = _rnd.Next(2000, 4000),
                    DamageRankActive = _rnd.Next(2000, 4000),

                });
            }

            return data;
        }

        public IEnumerable<ProfileData> GetUserProfiles()
        {
            return GetFavouriteProfiles();
        }
    }
}

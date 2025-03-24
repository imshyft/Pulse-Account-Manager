using LiveChartsCore.Motion;
using Studio.Contracts.Services;
using Studio.Helpers;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Services.Data.ProfileFetching
{
    class NoApiProfileFetchingService : IProfileFetchingService
    {

        public Task<ProfileFetchResult> GetUserProfile(BattleTag battletag)
        {
            Debug.WriteLine("Got Profile Details!");
            var profileFetchResult = new ProfileFetchResult()
            {
                Error = "",
                Profile = new Profile()
                {
                    Avatar = null,
                    Battletag = battletag,
                    CustomId = "Added Account",
                    Email = null,
                    LastUpdate = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    RankedCareer = new RankedCareer()
                    {
                        Damage = null,
                        Tank = null,
                        Support = null,
                    },
                    TimesSwitched = 0,
                    
                }
            };

            return Task.FromResult(profileFetchResult);
        }
    }
}

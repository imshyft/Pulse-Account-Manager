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

        public Task<ProfileFetchResult> FetchProfileAsync(BattleTagV2 battletag)
        {
            Debug.WriteLine("Got Profile Details!");
            var profileFetchResult = new ProfileFetchResult()
            {
                Outcome = ProfileFetchOutcome.Success,
                Profile = new ProfileV2()
                {
                    AvatarURL = null,
                    Battletag = battletag,
                    CustomName = "Added Account",
                    Email = null,
                    Snapshots = null,
                    
                }
            };

            return Task.FromResult(profileFetchResult);
        }

        public Task<ProfileFetchResult> UpdateProfileAsync(ProfileV2 profile)
        {
            profile.Snapshots.Add(new ProfileSnapshotV2()
            {
                Timestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });


            var result = new ProfileFetchResult()
            {
                Outcome = ProfileFetchOutcome.Success,
                Profile = profile
            };

            return Task.FromResult(result);
        }
    }
}

using Studio.Helpers;
using Studio.Models;
using Studio.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Contracts.Services
{
    public interface IProfileFetchingService
    {
        public Task<ProfileFetchResult> FetchProfileAsync(BattleTagV2 battletag);

        public Task<ProfileFetchResult> UpdateProfileAsync(ProfileV2 profile);
    }

    public class ProfileFetchResult
    {
        public ProfileV2 Profile { get; set; }
        public ProfileFetchOutcome Outcome { get; set; }
        public string? ErrorMessage { get; set; }

    }

    public enum ProfileFetchOutcome
    {
        Success,
        NotFound,
        Error
    }
}

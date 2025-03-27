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
        public Task<ProfileFetchResult> FetchProfileAsync(BattleTag battletag);
    }

    public class ProfileFetchResult
    {
        public Profile Profile { get; set; }
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

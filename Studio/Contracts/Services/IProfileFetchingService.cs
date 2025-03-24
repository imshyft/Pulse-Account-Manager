using Studio.Helpers;
using Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Contracts.Services
{
    public interface IProfileFetchingService
    {
        public Task<ProfileFetchResult> GetUserProfile(BattleTag battletag);
    }
}

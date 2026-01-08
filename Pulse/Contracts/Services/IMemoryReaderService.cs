using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Contracts.Services
{
    public interface IMemoryReaderService
    {
        Task<string[]> GetFriendBattletagStrings(IntPtr processHandle, CancellationToken token);
        /// <summary>
        /// Gets a list of recently logged into account Battletags from memory
        /// </summary>
        /// <param name="processHandle"></param>
        /// <returns>String Array of battletags. If the array is of length 1,
        /// it should be assumed to be the current logged in account.</returns>
        Task<string[]> GetUserBattletagStrings(IntPtr processHandle, CancellationToken token);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.Contracts.Services
{
    public interface IMemoryReaderService
    {
        string[] GetFriendBattleTagStrings(IntPtr processHandle);
        string GetUserBattleTagString(IntPtr processHandle);
    }
}

using Studio.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Studio.Services.BattleNet
{
    public class RegexMatchingMemoryReader : IMemoryReaderService
    {
        private List<string> _storedFriendBattleTags = new List<string>();
        private string _storedUserBattleTag = "";

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, UIntPtr nSize, out UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UIntPtr VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer, UIntPtr dwLength);

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        static readonly byte[] AccountMatchingPattern = { 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x5B};

        // Offset from pattern to name
        const long BACKWARD_OFFSET_TO_BASE = 0x4C;
        const int MAX_NAME_CHARS = 16;
        const long FAVOURITED_OFFSET = 0x30;

        public string[] GetFriendBattleTagStrings(IntPtr processHandle)
        {
            if (_storedFriendBattleTags.Count == 0)
                UpdateStoredResults(processHandle);
            return _storedFriendBattleTags.ToArray();
        }

        public string GetUserBattleTagString(IntPtr processHandle)
        {
            if (_storedUserBattleTag == "")
                UpdateStoredResults(processHandle);
            return _storedUserBattleTag;
        }

        private void UpdateStoredResults(IntPtr processHandle)
        {
            if (TryExtractBattleTags(processHandle, out var friends, out string user))
            {
                _storedFriendBattleTags = friends.Select(x => x.tag).ToList();
                _storedUserBattleTag = user;
            }
        }

        // Finds any battletag like string in memory
        /// TODO: Add option to search from any battletag in memory
        public bool TryReadAllBattleTagsInMemory(IntPtr hProc, out List<string> battletags)
        {
            List<string> cleanedTags = new();
            try
            {
                HashSet<string> rawTags = new();
                ReadProcessMemory(hProc, (MEMORY_BASIC_INFORMATION mbi, byte[] buffer) =>
                {
                    string pattern = @"(?<![\w\d])([a-z]\w{2,11}#\d{3,5})";
                    foreach (var val in MatchRegex(buffer, pattern))
                        rawTags.Add(val);
                    return true;
                });

                // clean shifted variants where reading starts inside a tag testname#1234
                // giving results like estname#1234
                cleanedTags = rawTags
                    .Where(s => char.IsLetter(s[0])) // if shift results in non letter first, discard (against naming rules)
                    .OrderByDescending(s => s.Length)
                    .ToList();

                for (int i = 0; i < cleanedTags.Count; i++)
                    for (int j = i + 1; j < cleanedTags.Count; j++)
                        if (cleanedTags[i].EndsWith(cleanedTags[j], StringComparison.OrdinalIgnoreCase))
                        {
                            cleanedTags.RemoveAt(j);
                            j--;
                        }

            }
            catch (Exception e)
            {
                battletags = new();
                return false;
            }

            battletags = cleanedTags;
            return true;

        }

        private static bool TryExtractBattleTags(IntPtr hProc, out List<(string tag, bool favourite)> friends, out string userTag)
        {
            bool success;
            var friendMatches = new List<(string name, bool favourite)>();
            string userName = "";

            try
            {
                var candidateAccountAddress = new List<IntPtr>();

                // find all potential account adresses
                ReadProcessMemory(hProc, (MEMORY_BASIC_INFORMATION mbi, byte[] buffer) =>
                {
                    foreach (int idx in FindAllOccurrences(buffer, AccountMatchingPattern))
                    {
                        IntPtr matchAddr = new IntPtr(mbi.BaseAddress.ToInt64() + idx);
                        candidateAccountAddress.Add(matchAddr);
                    }

                    return true;
                });

                // get names of accounts (not including id number)
                foreach (var addr in candidateAccountAddress)
                {
                    IntPtr baseAddr = new IntPtr(addr.ToInt64() - BACKWARD_OFFSET_TO_BASE);
                    string name = ReadASCIIString(hProc, baseAddr, MAX_NAME_CHARS);
                    string friendAccountPatternRegion = ReadASCIIString(hProc, new IntPtr(baseAddr.ToInt64() + 0x24), 4);
                    TryReadInt32At(hProc, new IntPtr(baseAddr.ToInt64() + 0x28), out int friendAccount);
                    TryReadInt32At(hProc, new IntPtr(baseAddr.ToInt64() + FAVOURITED_OFFSET), out int favourited);
                    if (name.Length > 0 && name.Length < 13)
                    {
                        if (friendAccount == 1)
                        {
                            bool isFavourite = favourited == 1;
                            friendMatches.Add((name.Trim('\0'), isFavourite));
                        }
                        else
                            userName = name;
                    }
                }

                // find id for each account
                ReadProcessMemory(hProc, (MEMORY_BASIC_INFORMATION mbi, byte[] buffer) =>
                {
                    bool allDataFilled = true;
                    for (int i = 0; i < friendMatches.Count; i++)
                    {
                        if (friendMatches[i].name.Contains('#')) continue;
                        foreach (int idx in FindAllOccurrences(buffer, Encoding.ASCII.GetBytes(friendMatches[i].name + "#")))
                        {
                            IntPtr matchAddr = new IntPtr(mbi.BaseAddress.ToInt64() + idx);
                            if (TryParseAccountTag(hProc, matchAddr, friendMatches[i].name, out string battleTag))
                                friendMatches[i] = (battleTag, friendMatches[i].favourite);
                        }
                        allDataFilled = false;
                    }

                    if (userName.Length > 0)
                    {
                        foreach (int idx in FindAllOccurrences(buffer, Encoding.ASCII.GetBytes(userName + "#")))
                        {
                            IntPtr matchAddr = new IntPtr(mbi.BaseAddress.ToInt64() + idx);
                            if (TryParseAccountTag(hProc, matchAddr, userName, out string battleTag))
                                userName = battleTag;
                        }
                        allDataFilled = false;
                    }

                    return !allDataFilled; // stop reading if no more to fill
                });

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                userTag = userName;
                friends = friendMatches;
            }

            return success;
        }

        private static bool TryParseAccountTag(IntPtr hProc, IntPtr address, string name, out string tag)
        {
            tag = "";
            string result = ReadASCIIString(hProc, address, 20);
            var regexMatch = Regex.Match(result, $@"{name}#\d+");

            if (regexMatch.Success)
                tag = regexMatch.Value;

            return regexMatch.Success;
        }

        private static bool TryReadInt32At(IntPtr hProc, IntPtr address, out int value)
        {
            byte[] buf = new byte[4];
            value = -1;
            if (ReadProcessMemory(hProc, address, buf, (UIntPtr)4, out UIntPtr bytesRead) && (long)bytesRead == 4)
            {
                value = BitConverter.ToInt32(buf, 0);
                return true;
            }
            return false;
        }

        private static void ReadProcessMemory(IntPtr hProc, Func<MEMORY_BASIC_INFORMATION, byte[], bool> OnNewBuffer)
        {
            IntPtr address = IntPtr.Zero;
            UIntPtr mbiSize = (UIntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

            byte[] prevBuffer = [];
            while (true)
            {
                UIntPtr ret = VirtualQueryEx(hProc, address, out MEMORY_BASIC_INFORMATION mbi, mbiSize);
                if (ret == UIntPtr.Zero) break;

                // Check  region state and protection
                const uint MEM_COMMIT = 0x1000;
                const uint PAGE_GUARD = 0x100;
                const uint PAGE_NOACCESS = 0x01;

                bool isCommitted = (mbi.State & MEM_COMMIT) != 0;
                bool isReadable = (mbi.Protect & PAGE_NOACCESS) == 0 && (mbi.Protect & PAGE_GUARD) == 0;

                if (isCommitted && isReadable)
                {
                    long regionSize = (long)mbi.RegionSize;
                    if (regionSize > 0)
                    {
                        byte[] buffer = new byte[regionSize];
                        if (ReadRegion(hProc, mbi.BaseAddress, buffer, out UIntPtr bytesRead) && (long)bytesRead > 0)
                        {
                            if (OnNewBuffer(mbi, buffer) == false)
                                return;
                            //prevBuffer = buffer;
                        }
                    }
                }

                // next region start
                long nextAddr = mbi.BaseAddress.ToInt64() + (long)mbi.RegionSize;
                if (nextAddr >= long.MaxValue) break;
                address = new IntPtr(nextAddr);
            }
        }

        private static bool ReadRegion(IntPtr hProc, IntPtr baseAddress, byte[] buffer, out UIntPtr bytesRead)
        {
            return ReadProcessMemory(hProc, baseAddress, buffer, (UIntPtr)buffer.Length, out bytesRead);
        }

        private static List<int> FindAllOccurrences(byte[] buffer, byte[] pattern)
        {
            var list = new List<int>();
            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j]) { ok = false; break; }
                }
                if (ok) list.Add(i);
            }
            return list;
        }

        private static string ReadASCIIString(IntPtr hProc, IntPtr address, int maxChars)
        {
            if (maxChars <= 0) return string.Empty;
            // read bytes for maxChars (2 bytes per char)
            int bytesToRead = maxChars * 2;
            byte[] buf = new byte[bytesToRead];

            if (!ReadProcessMemory(hProc, address, buf, (UIntPtr)buf.Length, out UIntPtr bytesRead) || (long)bytesRead == 0)
                return "<read failed>";

            // find null terminator (two consecutive zero bytes)
            int lenBytes = 0;
            for (int i = 0; i < (int)bytesRead - 1; i += 2)
            {
                if (buf[i] == 0 && buf[i + 1] == 0) { break; }
                lenBytes += 2;
            }

            if (lenBytes == 0) return string.Empty;
            return Encoding.ASCII.GetString(buf, 0, lenBytes);
        }

        static IEnumerable<string> MatchRegex(byte[] buffer, string pattern)
        {
            // Convert only printable bytes to ASCII, replace non-printables with space
            var clean = new StringBuilder(buffer.Length);
            foreach (byte b in buffer)
                clean.Append((b >= 32 && b <= 126) ? (char)b : ' ');

            string data = clean.ToString();

            //string pattern = @"(?<![A-Za-z0-9#])[A-Za-z][A-Za-z0-9]{1,11}#[0-9]{3,8}(?![A-Za-z0-9#])";
            foreach (Match m in Regex.Matches(data, pattern))
                yield return m.Value;
        }
    }
}

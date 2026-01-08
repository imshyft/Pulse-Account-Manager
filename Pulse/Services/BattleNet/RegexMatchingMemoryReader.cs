using Studio.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Studio.Services.BattleNet
{
    public class RegexMatchingMemoryReader : IMemoryReaderService
    {
        private static readonly string _memoryBattletagRegexPattern = @"(?<![\w\d])(([a-z]|[A-Z])\w{2,11}#\d{3,5})";

        #region DLL IMPORTS
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
        #endregion

        public Task<string[]> GetFriendBattletagStrings(IntPtr processHandle, CancellationToken token)
        {

            return Task.Run(() =>
            {
                if (TryExtractFriendAccounts(processHandle, out var accounts, token))
                {
                    return accounts.ToArray();
                }
                else return [];
            });

        }

        public Task<string[]> GetUserBattletagStrings(IntPtr processHandle, CancellationToken token)
        {

            return Task.Run(() =>
            {
                if (TryExtractRecentAccounts(processHandle, out var accounts, token))
                {
                    return accounts.ToArray();
                }
                else return [];
            });

        }


        private static bool TryExtractRecentAccounts(IntPtr hProc, out List<string> userAccounts, CancellationToken token)
        {
            bool success;
            HashSet<string> accounts = [];
            int[] b = { 0x07, 0x2D, 0x35, -1, 0x06, 0x04, 0x23, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0x2E, 0x61, 0x63, 0x74, 0x75, 0x61, 0x6C, 0x2E, 0x62, 0x61, 0x74, 0x74, 0x6C, 0x65, 0x2E, 0x6E, 0x65, 0x74 };

            try
            {

                ReadProcessMemory(hProc, (MEMORY_BASIC_INFORMATION mbi, byte[] buffer) =>
                {
                    token.ThrowIfCancellationRequested();

                    foreach (int idx in FindAllOccurrences(buffer, b))
                    {

                        string name = Encoding.ASCII.GetString(buffer, idx + 0x08, 0x34).Split("actual.battle.net").Last();

                        foreach (Match m in Regex.Matches(name, _memoryBattletagRegexPattern))
                        {
                            accounts.Add(m.Value);
                        }
                    }

                    return true;
                });

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                userAccounts = accounts.ToList();
            }

            return success;
        }

        private static bool TryExtractFriendAccounts(IntPtr hProc, out List<string> friends, CancellationToken token)
        {
            bool success = false;
            var friendMatches = new HashSet<string>(); // use hashset to avoid duplicates in memory
            int[] memoryPattern = [0x1A, -1, 0x0A, 0x0A, 0x08, 0xCE, 0x84, 0x01, 0x10, 0x01, 0x18, 0x04, 0x20, 0x00, 0x12, -1, 0x22, -1];
            try
            {
                ReadProcessMemory(hProc, (MEMORY_BASIC_INFORMATION mbi, byte[] buffer) =>
                {
                    token.ThrowIfCancellationRequested();

                    foreach (int idx in FindAllOccurrences(buffer, memoryPattern))
                    {
                        string rawMemoryString = Encoding.ASCII.GetString(buffer, idx + 0x12, 16);

                        foreach (Match m in Regex.Matches(rawMemoryString, _memoryBattletagRegexPattern))
                        {
                            friendMatches.Add(m.Value);
                        }
                    }

                    return true;
                });
                success = true;

            }
            catch (Exception ex)
            {
                success = false;
            }
            finally
            {
                friends = [.. friendMatches];
            }

            return success;
        }

        private static void ReadProcessMemory(IntPtr hProc, Func<MEMORY_BASIC_INFORMATION, byte[], bool> OnNewBuffer, int overlapBytes = 256)
        {
            IntPtr address = IntPtr.Zero;
            UIntPtr mbiSize = (UIntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

            byte[] prevTail = Array.Empty<byte>();

            while (true)
            {
                UIntPtr ret = VirtualQueryEx(hProc, address, out MEMORY_BASIC_INFORMATION mbi, mbiSize);
                if (ret == UIntPtr.Zero)
                    break;

                const uint MEM_COMMIT = 0x1000;
                const uint PAGE_GUARD = 0x100;
                const uint PAGE_NOACCESS = 0x01;

                bool isCommitted = (mbi.State & MEM_COMMIT) != 0;
                bool isReadable =
                    (mbi.Protect & PAGE_NOACCESS) == 0 &&
                    (mbi.Protect & PAGE_GUARD) == 0;

                if (isCommitted && isReadable)
                {
                    long regionSize = (long)mbi.RegionSize;
                    if (regionSize > 0)
                    {
                        byte[] buffer = new byte[regionSize];

                        if (ReadRegion(hProc, mbi.BaseAddress, buffer, out UIntPtr bytesRead) &&
                            (long)bytesRead > 0)
                        {
                            int read = (int)bytesRead;

                            // Combine previous tail and buffer
                            byte[] combined;
                            if (prevTail.Length > 0)
                            {
                                combined = new byte[prevTail.Length + read];
                                Buffer.BlockCopy(prevTail, 0, combined, 0, prevTail.Length);
                                Buffer.BlockCopy(buffer, 0, combined, prevTail.Length, read);
                            }
                            else
                            {
                                combined = buffer.AsSpan(0, read).ToArray();
                            }

                            if (OnNewBuffer(mbi, combined) == false)
                                return;

                            // Save tail for next iteration
                            int tailSize = Math.Min(overlapBytes, read);
                            prevTail = new byte[tailSize];
                            Buffer.BlockCopy(buffer, read - tailSize, prevTail, 0, tailSize);
                        }
                    }
                }
                else
                {
                    // Reset overlap if region is unreadable
                    prevTail = Array.Empty<byte>();
                }

                long nextAddr = mbi.BaseAddress.ToInt64() + (long)mbi.RegionSize;
                if (nextAddr <= 0 || nextAddr >= long.MaxValue)
                    break;

                address = new IntPtr(nextAddr);
            }
        }

        private static bool ReadRegion(IntPtr hProc, IntPtr baseAddress, byte[] buffer, out UIntPtr bytesRead)
        {
            return ReadProcessMemory(hProc, baseAddress, buffer, (UIntPtr)buffer.Length, out bytesRead);
        }

        // Find pattern in buffer, where -1 in the pattern indicates to skip comparing the index
        private static List<int> FindAllOccurrences(byte[] buffer, int[] pattern)
        {

            var list = new List<int>();
            for (int i = 0; i <= buffer.Length - pattern.Length; i++)
            {
                bool valid = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j] == -1)
                        continue;

                    if (buffer[i + j] != pattern[j]) { valid = false; break; }
                }
                if (valid) list.Add(i);
            }
            return list;
        }
    }
}

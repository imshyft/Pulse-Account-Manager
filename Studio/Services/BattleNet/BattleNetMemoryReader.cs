using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Studio.Models;

namespace Studio.Services.BattleNet
{
    public class BattleNetMemoryReaderService
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private bool isBattleTag(string battletagString)
        {
            if (string.IsNullOrEmpty(battletagString))
                return false;

            string[] parts = battletagString.Split("#");
            if (parts.Length != 2) return false;

            if (!int.TryParse(parts[1], out _))
                return false;

            return true;
        }
        public BattleTag? FindBattleTagInMemory()
        {
            List<BattleNetMemoryStringQuery> queries = new List<BattleNetMemoryStringQuery>()
            {
                new BattleNetMemoryStringQuery() { Reference = "x-bnet-authenticate", Offset = 80, Length = 18},
                new BattleNetMemoryStringQuery() { Reference = "streamSubscriptions", Offset = 40, Length = 18},
                new BattleNetMemoryStringQuery() { Reference = "ClubTicketRangeSet", Offset = 80, Length= 18},
                new BattleNetMemoryStringQuery() { Reference = "num_messages_sent", Offset = -40, Length= 18},
                new BattleNetMemoryStringQuery() { Reference = "K_ICON_SHOP", Offset = 23, Length= 18},
            };

            Process process = Process.GetProcessesByName("Battle.Net")[0];
            return FindUnknownString(process.Id, queries.ToArray());
        }


        public BattleTag? FindUnknownString(int processId, params BattleNetMemoryStringQuery[] queries)
        {
            IntPtr hProcess = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                Debug.WriteLine("Failed to open process.");
                return null;
            }

            IntPtr currentAddr = IntPtr.Zero;
            MEMORY_BASIC_INFORMATION mbi;

            while (VirtualQueryEx(hProcess, currentAddr, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))))
            {
                if ((mbi.Protect & 0x04) != 0) // If memory is PAGE_READWRITE
                {
                    byte[] buffer = new byte[(int)mbi.RegionSize];
                    IntPtr bytesRead;

                    if (ReadProcessMemory(hProcess, mbi.BaseAddress, buffer, (uint)buffer.Length, out bytesRead))
                    {
                        string regionText = Encoding.ASCII.GetString(buffer);

                        foreach (var query in queries)  // Loop through all queries
                        {
                            int index = 0;
                            while ((index = regionText.IndexOf(query.Reference, index, StringComparison.Ordinal)) != -1)
                            {
                                int unknownStart = index + query.Offset;
                                if (unknownStart >= 0 && unknownStart + query.Length <= buffer.Length)
                                {
                                    string extracted = Encoding.ASCII.GetString(buffer, unknownStart, query.Length);
                                    Debug.WriteLine($"Found Match: {extracted}, using query {query.Reference}{(query.Offset > 0 ? '+' : "")}{query.Offset}");

                                    if (isBattleTag(extracted))
                                    {
                                        Debug.WriteLine($"{extracted} is a valid BattleTag");
                                        CloseHandle(hProcess);
                                        return new BattleTag(extracted.Replace("\0", "")); // Fix null terminators
                                    }
                                }
                                index += query.Reference.Length;
                            }
                        }
                    }
                }
                currentAddr = (IntPtr)((long)mbi.BaseAddress + (long)mbi.RegionSize);
            }

            CloseHandle(hProcess);
            return null;
        }

        public class BattleNetMemoryStringQuery
        {
            public string Reference;
            public int Offset;
            public int Length;
        }
    }
}

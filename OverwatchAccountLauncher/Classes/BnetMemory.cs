using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OverwatchAccountLauncher.Classes
{
    public struct BlizzardFriend
    {
        public string Name;
        public bool Favourite;
    }

    class Mem
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PAGE_READWRITE = 0x04;

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public static void Test()
        {
            IntPtr handle = OpenProcess(0x1F0FFF, false, Process.GetCurrentProcess().Id);

            MEMORY_BASIC_INFORMATION mbi;
            IntPtr currentAddr = IntPtr.Zero;
            List<BlizzardFriend> users = new List<BlizzardFriend>();
            BlizzardFriend self = new BlizzardFriend();

            byte[] accountBuffer = new byte[276];
            byte[] accountDataBuffer = new byte[0xFF];
            byte[] accountSubData = new byte[0xFF];
            byte[] temp = new byte[0xFF];


            while (VirtualQueryEx(handle, currentAddr, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))))
            {
                if (mbi.Protect == PAGE_READWRITE)
                {
                    byte[] buffer = new byte[(uint)mbi.RegionSize + 1];
                    ReadProcessMemory(handle, currentAddr, buffer, (uint)mbi.RegionSize, out _);

                    for (uint i = 0; i < mbi.RegionSize - 8; i += 4)
                    {


                        if (BitConverter.ToUInt32(buffer, (int)(i + 0x1C)) != 0 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x2C)) == 7 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x30)) == 8 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x48)) == 0 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x54)) == 0 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x58)) == 1 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x5C)) == 0 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x60)) == 0 &&
                            BitConverter.ToUInt32(buffer, (int)(i + 0x88)) <= 4)
                        {
                            BlizzardFriend u = new BlizzardFriend
                            {
                                Name = System.Text.Encoding.UTF8.GetString(buffer, (int)(i + 0x8C), 256).TrimEnd('\0'),
                                Favourite = Convert.ToBoolean(buffer[i + 0xC0])
                            };
                            if (BitConverter.ToUInt32(buffer, (int)(i + 0xB8)) == 0) // self
                            {
                                self = u;
                            }
                            else
                            {
                                // Add user to list
                                users.Add(u);
                            }
                        }
                    }
                }
                currentAddr = IntPtr.Add(currentAddr, (int)mbi.RegionSize);
            }
            CloseHandle(handle);
        }
    }
}

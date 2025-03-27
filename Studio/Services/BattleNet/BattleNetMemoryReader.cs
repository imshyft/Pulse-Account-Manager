using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Studio.Models;

namespace Studio.Services.BattleNet
{
    public class BattleNetMemoryReaderService
    {
        private const string _dllImportPath = @"BlizzardMemoryReader.dll";

        [DllImport(_dllImportPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SanityCheck();



        [DllImport(_dllImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetFriendBattleTags(IntPtr processHandle, out int outSize);

        [DllImport(_dllImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetUserBattleTag(IntPtr processHandle, out int outSize);


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

        public static string[] GetFriendBattleTags(IntPtr processHandle)
        {
            int size;
            IntPtr ptrArray = GetFriendBattleTags(processHandle, out size);

            string[] result = new string[size];

            for (int i = 0; i < size; i++)
            {
                IntPtr strPtr = Marshal.ReadIntPtr(ptrArray, i * IntPtr.Size); 
                result[i] = Marshal.PtrToStringAnsi(strPtr);
            }


            VirtualFree(ptrArray, 0, 0x8000); // 0x8000 = MEM_RELEASE, to free the allocated memory

            return result;
        }

        public static string GetUserBattleTag(IntPtr processHandle)
        {
            int size;
            IntPtr ptrArray = GetUserBattleTag(processHandle, out size);

            string[] result = new string[size];

            for (int i = 0; i < size; i++)
            {
                IntPtr strPtr = Marshal.ReadIntPtr(ptrArray, i * IntPtr.Size);
                result[i] = Marshal.PtrToStringAnsi(strPtr);
            }


            VirtualFree(ptrArray, 0, 0x8000); // 0x8000 = MEM_RELEASE, to free the allocated memory

            if (result.Length > 0)
                return result[0];
            else return null;
        }
    }
}

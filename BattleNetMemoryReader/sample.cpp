#include "pch.h"
#include "sample.h"
#include <iostream>
#include <string>
#include <list>
#include <fstream>
#include <vector>

#include <stdio.h>
#include <Windows.h>
#include <winver.h>
#include <psapi.h>
#include <TlHelp32.h>

using namespace std;

char* formatBlizzardString(HANDLE Handle, char* obuffer, char* ibuffer, uint32_t strOffset)
{
    uint32_t stringLen = *(uint32_t*)(ibuffer + strOffset + 0x10);
    uint32_t allocLen = *(uint32_t*)(ibuffer + strOffset + 0x14);

    if (stringLen == 0 || stringLen > 0xFF || allocLen > 0xFF)
    {
        obuffer[0] = '\0';
        return obuffer;
    }

    if (allocLen == 0xF)
    {
        memcpy(obuffer, ibuffer + strOffset, stringLen);
    }
    else
    {
        ReadProcessMemory(Handle, (char*)*(uint32_t*)(ibuffer + strOffset), obuffer, stringLen, NULL);
    }
    obuffer[stringLen] = '\0';
    return obuffer;
}

extern "C" _declspec(dllexport) const char** GetFriendBattleTags(HANDLE Handle, int* outSize) {
    static std::vector<std::string> words;  // Store strings properly
    static std::vector<std::uint32_t> addresses;

    MEMORY_BASIC_INFORMATION mbi;
    uint64_t currentAddr = 0;

    char accountBuffer[276];
    char accountDataBuffer[0xff];
    char accountSubData[0xFF];
    char temp[0xFF];

    words.clear(); 
    addresses.clear();



    while (VirtualQueryEx(Handle, (char*)currentAddr, &mbi, sizeof(mbi)))
    {
        if (mbi.Protect & PAGE_READWRITE)
        {
            char* buffer = new char[mbi.RegionSize + 1];
            ReadProcessMemory(Handle, (char*)currentAddr, buffer, mbi.RegionSize, NULL);

            for (size_t i = 0; i < mbi.RegionSize - 8; i += 4)
            {
                char* accountBufferPtr = buffer + i;

                if (*(uint32_t*)(accountBufferPtr + 0x1C) != 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x2C) == 7 &&
                    *(uint32_t*)(accountBufferPtr + 0x30) == 8 &&
                    *(uint32_t*)(accountBufferPtr + 0x48) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x54) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x58) == 1 &&
                    *(uint32_t*)(accountBufferPtr + 0x5C) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x60) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x88) <= 4 &&
                    *(uint32_t*)(accountBufferPtr + 0xB8) != 0) // 0 means self

                {
                    addresses.push_back(currentAddr + i);
                }
            }
            delete[] buffer;
        }
        currentAddr += mbi.RegionSize;
    }

    for (const uint32_t& i : addresses) {
        ReadProcessMemory(Handle, (char*)(i), accountBuffer, 275, NULL);
        ReadProcessMemory(Handle, (char*)*(uint32_t*)(accountBuffer + 0x68), accountDataBuffer, 0xFE, NULL);
        ReadProcessMemory(Handle, (char*)*(uint32_t*)(accountDataBuffer + 0x08), accountSubData, 0xFE, NULL);

        char* name = formatBlizzardString(Handle, temp, accountSubData, 0x00);
        words.push_back(std::string(name)); 
    }

    *outSize = words.size();  // Set the number of strings
    const char** result = new const char*[*outSize];  // Allocate memory for string pointers

    // Copy the strings into the result array
    for (int i = 0; i < *outSize; ++i) {
        result[i] = words[i].c_str();  // Store pointer to each string
    }

    // Return the array of string pointers
    return result;

    CloseHandle(Handle);
}

extern "C" _declspec(dllexport) const char** GetUserBattleTag(HANDLE Handle, int* outSize) {
    static std::vector<std::string> words;  // Store strings properly
    static std::vector<std::uint32_t> addresses;

    MEMORY_BASIC_INFORMATION mbi;
    uint64_t currentAddr = 0;

    char accountBuffer[276];
    char accountDataBuffer[0xff];
    char accountSubData[0xFF];
    char temp[0xFF];

    words.clear();
    addresses.clear();



    while (VirtualQueryEx(Handle, (char*)currentAddr, &mbi, sizeof(mbi)))
    {
        if (mbi.Protect & PAGE_READWRITE)
        {
            char* buffer = new char[mbi.RegionSize + 1];
            ReadProcessMemory(Handle, (char*)currentAddr, buffer, mbi.RegionSize, NULL);

            for (size_t i = 0; i < mbi.RegionSize - 8; i += 4)
            {
                char* accountBufferPtr = buffer + i;

                if (*(uint32_t*)(accountBufferPtr + 0x1C) != 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x2C) == 7 &&
                    *(uint32_t*)(accountBufferPtr + 0x30) == 8 &&
                    *(uint32_t*)(accountBufferPtr + 0x48) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x54) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x58) == 1 &&
                    *(uint32_t*)(accountBufferPtr + 0x5C) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x60) == 0 &&
                    *(uint32_t*)(accountBufferPtr + 0x88) <= 4 &&
                    *(uint32_t*)(accountBufferPtr + 0xB8) == 0) // 0 means self

                {
                    addresses.push_back(currentAddr + i);
                }
            }
            delete[] buffer;
        }
        currentAddr += mbi.RegionSize;
    }

    for (const uint32_t& i : addresses) {
        ReadProcessMemory(Handle, (char*)(i), accountBuffer, 275, NULL);
        ReadProcessMemory(Handle, (char*)*(uint32_t*)(accountBuffer + 0x68), accountDataBuffer, 0xFE, NULL);
        ReadProcessMemory(Handle, (char*)*(uint32_t*)(accountDataBuffer + 0x08), accountSubData, 0xFE, NULL);

        char* name = formatBlizzardString(Handle, temp, accountSubData, 0x00);
        words.push_back(std::string(name));
    }

    *outSize = words.size();  // Set the number of strings
    const char** result = new const char* [*outSize];  // Allocate memory for string pointers

    // Copy the strings into the result array
    for (int i = 0; i < *outSize; ++i) {
        result[i] = words[i].c_str();  // Store pointer to each string
    }

    // Return the array of string pointers
    return result;

    CloseHandle(Handle);
}
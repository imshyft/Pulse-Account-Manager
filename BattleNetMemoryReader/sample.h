#pragma once
#include <vector>
#include <list>


extern "C" _declspec(dllexport) const char** GetFriendBattleTags(HANDLE Handle, int* outSize);
extern "C" _declspec(dllexport) const char** GetUserBattleTag(HANDLE Handle, int* outSize);
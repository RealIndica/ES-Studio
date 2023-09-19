#ifndef MEMORY_H
#define MEMORY_H

#include <Windows.h>
#include <Psapi.h>

#include <iomanip>
#include <sstream>
#include <iostream>

namespace Memory {
	uintptr_t getBase();
	uintptr_t rebase(DWORD address);
	uintptr_t rebaseIDA(DWORD address);
	uintptr_t FindPatternIDA(const char* szSignature);
	MODULEINFO GetModuleInfo();
	void WriteLogAddress(const char* name, uintptr_t addy, bool rebase = true);

}

#endif
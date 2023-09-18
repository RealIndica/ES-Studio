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
	
	template< typename T >
	const char* int_to_hex(T i) {
		std::stringstream stream;
		stream << "0x" << std::uppercase << std::hex << i;
		return stream.str().c_str();
	}
	
	template< typename T >
	void WriteLogAddress(const char* name, T addy, bool rebase = true) {
		if (rebase) {
			addy = (T)((unsigned __int64)addy - getBase());
		}
		const char* address = Memory::int_to_hex(addy);
		char buf[256];
		strcpy_s(buf, "[+] ");
		strcat_s(buf, name);
		strcat_s(buf, " : ");
		strcat_s(buf, address);
		strcat_s(buf, "\n");
		std::cout << buf;
	}
}

#endif
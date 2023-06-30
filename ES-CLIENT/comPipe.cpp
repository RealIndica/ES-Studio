#include "comPipe.h"

void pipeSendText(const char* text) {
    HANDLE hPipe;
    DWORD dwWritten;


    hPipe = CreateFile(TEXT("\\\\.\\pipe\\est-input-pipe"),
        GENERIC_READ | GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        0,
        NULL);
    if (hPipe != INVALID_HANDLE_VALUE)
    {
        WriteFile(hPipe,
            text,
            strlen(text),
            &dwWritten,
            NULL);

        CloseHandle(hPipe);
    }
}
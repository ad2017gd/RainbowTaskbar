#include "others.h"
#include "gui.h"

void FileDiag_Open(HWND hWnd,LPWSTR pathptr) {
    OPENFILENAME ofn = { 0 };
    LPWSTR szFile = malloc(257 * sizeof(WCHAR));
    memset(szFile, 0, 257 * sizeof(WCHAR));

    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hWnd;
    ofn.lpstrFile = szFile;
    ofn.nMaxFile = 256;
    ofn.lpstrFilter = L"Bitmap images\0*.BMP\0";
    ofn.nFilterIndex = 1;
    ofn.lpstrFileTitle = NULL;
    ofn.nMaxFileTitle = 0;
    ofn.lpstrInitialDir = NULL;
    ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

    if (GetOpenFileName(&ofn) == TRUE)
    {
        wcscpy(pathptr, szFile);
    }

    free(szFile);
}
void Class(wchar_t* name, HANDLE hInstance, WNDPROC proc) {
    WNDCLASS wc = { 0 };

    wc.lpfnWndProc = proc;
    wc.hInstance = hInstance;
    wc.lpszClassName = name;


    RegisterClass(&wc);

}
// pasted code from https://stackoverflow.com/questions/478898/how-do-i-execute-a-command-and-get-the-output-of-the-command-within-c-using-po
char* ExecCmd(LPWSTR cmd, char* buf)
{
    char* strResult = NULL;
    HANDLE hPipeRead, hPipeWrite;

    SECURITY_ATTRIBUTES saAttr = { sizeof(SECURITY_ATTRIBUTES) };
    saAttr.bInheritHandle = TRUE; // Pipe handles are inherited by child process.
    saAttr.lpSecurityDescriptor = NULL;

    // Create a pipe to get results from child's stdout.
    if (!CreatePipe(&hPipeRead, &hPipeWrite, &saAttr, 0))
        return strResult;

    STARTUPINFOW si = { 0 };
    si.cb = sizeof(STARTUPINFOW);
    si.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
    si.hStdOutput = hPipeWrite;
    si.hStdError = hPipeWrite;
    si.wShowWindow = SW_HIDE; // Prevents cmd window from flashing.
                              // Requires STARTF_USESHOWWINDOW in dwFlags.

    PROCESS_INFORMATION pi = { 0 };

    LPWSTR temp = malloc(sizeof(WCHAR) * wcslen(cmd));
    CreateProcessW(NULL, temp, NULL, NULL, TRUE, CREATE_NEW_CONSOLE, NULL, NULL, &si, &pi);


    BOOL bProcessEnded = FALSE;
    for (; !bProcessEnded;)
    {
        // Give some timeslice (50 ms), so we won't waste 100% CPU.
        bProcessEnded = WaitForSingleObject(pi.hProcess, 50) == WAIT_OBJECT_0;

        // Even if process exited - we continue reading, if
        // there is some data available over pipe.
        for (;;)
        {
            DWORD dwRead = 0;
            DWORD dwAvail = 0;

            if (!PeekNamedPipe(hPipeRead, NULL, 0, NULL, &dwAvail, NULL))
                break;

            if (!dwAvail) // No data available, return
                break;

            if (!ReadFile(hPipeRead, buf, min(1024 - 1, dwAvail), &dwRead, NULL) || !dwRead)
                // Error, the child process might ended
                break;

            buf[dwRead] = 0;
            strResult = buf;
        }
    }

    CloseHandle(hPipeWrite);
    CloseHandle(hPipeRead);
    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    free(temp);
    return strResult;
}

COLORREF PickColor(int which) {
    CHOOSECOLOR cc = { 0 };
    COLORREF acrCustClr[16];
    HBRUSH hbrush;

    cc.lStructSize = sizeof(cc);
    cc.hwndOwner = mainn;
    cc.lpCustColors = (LPDWORD)acrCustClr;
    cc.rgbResult = which ? rgbCurr2 : rgbCurr;
    cc.Flags = CC_FULLOPEN | CC_RGBINIT;

    if (ChooseColor(&cc) == TRUE)
    {
        return cc.rgbResult;
    }
    return 0;
}


POINT Point_FromString(LPWSTR str) {
    POINT pt = { 0 };
    wchar_t* buf = malloc(1026);
    wchar_t* tok;

    tok = wcstok(str, L"x", buf);
    if (tok != NULL) swscanf(tok, L"%i", &pt.x);

    if (tok != NULL) tok = wcstok(NULL, L"x", buf);
    if (tok != NULL) swscanf(tok, L"%i", &pt.y);

    free(buf);

    return pt;
}

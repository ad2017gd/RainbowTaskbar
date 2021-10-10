#include "filediag.h"


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
#pragma once

#include <windows.h>
#include <shobjidl.h>

void FileDiag_Open(HWND hWnd, LPWSTR pathptr);
void Class(wchar_t* name, HANDLE hInstance, WNDPROC proc);
char* ExecCmd(const wchar_t* cmd, char* buf);
COLORREF PickColor(int which);
POINT Point_FromString(LPWSTR str);
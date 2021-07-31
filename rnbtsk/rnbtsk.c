// rainbow taskbar program thing
// by ad2017!!!
#define _CRT_SECURE_NO_WARNINGS

#include <Windows.h>
#include <synchapi.h>
#include <Uxtheme.h>
#include <tchar.h>
#include <stdio.h>
#include <stdlib.h>
#include "config.h"
#include "accent.h"

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
DWORD WINAPI Thrd(void* data);

COLORREF current = 0;
HWND hTaskBar;
int main() {

	FreeConsole();

	char* appdata = getenv("APPDATA");
	char* confpath[512];
	sprintf(confpath, "%s\\rnbconf.txt", appdata);

	FILE* fconfig = fopen(confpath, "r");
	TCHAR thisfile[MAX_PATH];
	GetModuleFileName(NULL, thisfile, MAX_PATH);
	if (!fconfig) {

		
		if (MessageBox(NULL, _T("Would you like to launch RainbowTaskbar at Windows startup? (will break if executable is moved or deleted)"), _T("RainbowTaskbar"), 0x00000004L) == 6) {
			HKEY hkey = NULL;
			RegCreateKey(HKEY_CURRENT_USER, _T("Software\\Microsoft\\Windows\\CurrentVersion\\Run"), &hkey);
			RegSetValueEx(hkey, _T("RainbowTaskbar"), 0, REG_SZ, thisfile, (_tcslen(thisfile) + 1) * sizeof(TCHAR));
			RegCloseKey(hkey);

		}
		fconfig = fopen(confpath, "wb+");
		fwrite(_RNBDEFCONFIG, sizeof(char), strlen(_RNBDEFCONFIG), fconfig); 

		
	}
	fclose(fconfig);
	fconfig = fopen(confpath, "rb");
	char* _ln = malloc(512);
	rtcfg* cfg = malloc(2048*sizeof(rtcfg_step));
	int i = 0;
	while (fgets(_ln, 512, fconfig)) {
		if (_ln[0] == '#' || _ln[0] == '\r' || _ln[0] == '\n') continue;

		rtcfg_step step;
		sscanf(_ln, "%c %i %i %i %i %s %i %i %i", &step.prefix, &step.time, &step.r, &step.g, &step.b, &step.effect, &step.effect_1, &step.effect_2, &step.effect_3);
		step.effect[4] = '\0';
		char a[2];
		cfg->steps[i++] = step;
	}
	cfg->len = i;

	// begin

	LoadUX();
	//SetAccentColor(0x00000000);
	current = GetAccentColor();
	
	DWORD dwStyle;

	hTaskBar = FindWindow(_T("Shell_TrayWnd"), 0);
	dwStyle = GetWindowLong(hTaskBar, GWL_EXSTYLE);
	SetWindowLong(hTaskBar, GWL_EXSTYLE, WS_EX_LAYERED);
	SetLayeredWindowAttributes(hTaskBar, 0, 240, LWA_ALPHA);

	

	RECT tr;
	const wchar_t CLASS_NAME[] = L"RnbTskWnd";

	WNDCLASS wc = { 0 };

	wc.lpfnWndProc = WindowProc;
	wc.hInstance = GetModuleHandleA(NULL);
	wc.lpszClassName = CLASS_NAME;

	RegisterClass(&wc);
	GetWindowRect(hTaskBar, &tr);
	HWND cw = CreateWindowEx(
		WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
		CLASS_NAME,
		L"RainbowTaskbar",
		WS_POPUP | WS_VISIBLE | WS_SYSMENU,
		tr.left,
		tr.top,
		tr.right,
		tr.bottom,
		NULL,
		NULL,
		GetModuleHandleA(NULL),
		NULL
	);
	if (cw == NULL)
	{
		return 0;
	}
	CreateThread(NULL,
		0,
		Thrd,
		cfg,
		0,
		NULL);
	MSG msg = { 0 };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}



	
	free(_ln);
	free(cfg);
	fclose(fconfig);
	return 0;
}



DWORD WINAPI Thrd(void* data) {
	rtcfg* cfg = (rtcfg*)data;
	while (1) {
		for (int i = 0; i < cfg->len; i++) {

			rtcfg_step step = cfg->steps[i];
			if (step.prefix == 'c') {

				if (!strcmp(step.effect, "none")) {
					current = RGB(step.r, step.g, step.b);
					Sleep(step.time);

				}
				else if (!strcmp(step.effect, "fade")) {
					int j = 1;
					COLORREF last = current;
					while (j++ < step.effect_2) {
						current = clerp(last, RGB(step.r, step.g, step.b), lncv(j, 0, step.effect_2, 0, 1));
						Sleep(step.effect_1 / step.effect_2);
					}
					Sleep(step.time);
				}
			}
		}
	}
}

int idTimer = -1;
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_CREATE:
		SetTimer(hwnd, idTimer = 1, 1, NULL);
		RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE);
		SetWindowPos(hTaskBar, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		break;
	case WM_TIMER:
		RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE);
		break;
	
	case WM_DESTROY:
	{
		SetWindowBlur(hTaskBar, NULL);
		PostQuitMessage(0);
		return 0;
	}
	case WM_PAINT:
	{
		PAINTSTRUCT ps;
		HDC hdc = BeginPaint(hwnd, &ps);



		HBRUSH brush = CreateSolidBrush(current);
		SelectObject(hdc, brush);
		RECT tr = {0};
		GetWindowRect(hTaskBar, &tr);
		//SetWindowPos(hTaskBar, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		SetWindowBlur(hTaskBar, 4);

		SetWindowPos(hwnd, hTaskBar, tr.left, tr.top, tr.right, tr.bottom, SWP_NOREDRAW);
		FillRect(hdc, &ps.rcPaint, brush);
		EndPaint(hwnd, &ps);
		break;
	}
	return 0;

	}
	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
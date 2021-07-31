// rainbow taskbar program thing
// by ad2017!!!
#define _CRT_SECURE_NO_WARNINGS
#pragma comment(lib, "Msimg32.lib")

#include <Windows.h>
#include <synchapi.h>
#include <Uxtheme.h>
#include <tchar.h>
#include <stdio.h>
#include <stdlib.h>
#include <wingdi.h>
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
		sscanf(_ln, "%c %i %i %i %i %4s %i %i %i %i %i %i %8s", &step.prefix, &step.time, &step.r, &step.g, &step.b, &step.effect, &step.effect_1, &step.effect_2, &step.effect_3, &step.effect_4, &step.effect_5, &step.effect_6, &step.effect_7);
		step.effect[4] = '\0';
		char a[2];
		cfg->steps[i++] = step;
	}
	cfg->len = i;
	free(_ln);
	fclose(fconfig);
	// begin

	LoadUX();
	//SetAccentColor(0x00000000);
	//current = GetAccentColor();
	
	DWORD dwStyle;

	hTaskBar = FindWindow(_T("Shell_TrayWnd"), 0);

	
	
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
		WS_POPUP | WS_VISIBLE,
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



	
	
	free(cfg);
	return 0;
}

BOOL blur = FALSE;
HDC hdc;
HWND winhwnd;
TRIVERTEX vertex[2] = { 0 };
GRADIENT_RECT gRect;
BOOL gradient = FALSE;
RECT tr;

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
						current = clerp(last, RGB(step.r, step.g, step.b), (double)j/step.effect_2);
						Sleep(step.effect_1 / step.effect_2);
					}
					Sleep(step.time);
				}
				else if (!strcmp(step.effect, "grad")) {
					GetWindowRect(hTaskBar, &tr);
					
					vertex[0].x = 0;
					vertex[0].y = 0;
					vertex[0].Red = step.r << 8;
					vertex[0].Green = step.g << 8;
					vertex[0].Blue = step.b << 8;
					vertex[0].Alpha = 0xFFFF;

					vertex[1].x = tr.right;
					vertex[1].y = tr.bottom-tr.top ;
					vertex[1].Red = step.effect_1 << 8;
					vertex[1].Green = step.effect_2 << 8;
					vertex[1].Blue = step.effect_3 << 8;
					vertex[1].Alpha = 0xFFFF;

					gRect.UpperLeft = 1;
					gRect.LowerRight = 0;

					gradient = TRUE;
					Sleep(step.time);
					gradient = FALSE;
				}
				else if (!strcmp(step.effect, "fgrd")) {
					GetWindowRect(hTaskBar, &tr);
					COLORREF c1 = RGB(vertex[0].Red >> 8, vertex[0].Green >> 8, vertex[0].Blue >> 8);
					COLORREF c2 = RGB(vertex[1].Red >> 8, vertex[1].Green >> 8, vertex[1].Blue >> 8);
					int j = 1;

					while (j++ < step.effect_5) {
						COLORREF interp1 = clerp(c1, RGB(step.r, step.g, step.b), (double)j / step.effect_5);
						COLOR* rgb1 = (COLOR*)&interp1;
						vertex[0].x = 0;
						vertex[0].y = 0;
						vertex[0].Red = rgb1->R << 8;
						vertex[0].Green = rgb1->G << 8;
						vertex[0].Blue = rgb1->B << 8;
						vertex[0].Alpha = 0;

						COLORREF interp2 = clerp(c2, RGB(step.effect_1, step.effect_2, step.effect_3), (double)j / step.effect_5);
						COLOR* rgb2 = (COLOR*)&interp2;
						vertex[1].x = tr.right;
						vertex[1].y = tr.bottom - tr.top;
						vertex[1].Red = rgb2->R << 8;
						vertex[1].Green = rgb2->G << 8;
						vertex[1].Blue = rgb2->B << 8;
						vertex[1].Alpha = 0;
						gRect.UpperLeft = 1;
						gRect.LowerRight = 0;
						gradient = TRUE;
						Sleep(step.effect_4 / step.effect_5);
					}
					Sleep(step.time);
					gradient = FALSE;
				}
			}
			else if (step.prefix == 't') {

					switch (step.time) {
					case 1: {
						DWORD dwStyle = GetWindowLong(hTaskBar, GWL_EXSTYLE);
						SetWindowLong(hTaskBar, GWL_EXSTYLE, WS_EX_LAYERED);
						SetLayeredWindowAttributes(hTaskBar, 0, step.r, LWA_ALPHA);
						SetWindowPos(hTaskBar, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
						break;
					}
					case 2: {
						DWORD dwStyle = GetWindowLong(winhwnd, GWL_EXSTYLE);
						SetWindowLong(winhwnd, GWL_EXSTYLE, WS_EX_LAYERED);
						SetLayeredWindowAttributes(winhwnd, 0, step.r, LWA_ALPHA);
						SetWindowPos(winhwnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
						break;
					}
					case 3: {
						DWORD dwStyle = GetWindowLong(hTaskBar, GWL_EXSTYLE);
						SetWindowLong(hTaskBar, GWL_EXSTYLE, WS_EX_LAYERED);
						SetLayeredWindowAttributes(hTaskBar, 0, step.r, LWA_ALPHA);
						SetWindowPos(hTaskBar, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
						dwStyle = GetWindowLong(winhwnd, GWL_EXSTYLE);
						SetWindowLong(winhwnd, GWL_EXSTYLE, WS_EX_LAYERED);
						SetLayeredWindowAttributes(winhwnd, 0, step.r, LWA_ALPHA);
						SetWindowPos(winhwnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
						break;
						
					}
					case 4: {
						blur = TRUE;
						break;
					}
					}
				

			}
			else if (step.prefix == 'w') {
				Sleep(step.time);
			}

		}
	}
}

int idTimer = -1;
PAINTSTRUCT ps;
LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	winhwnd = hwnd;
	switch (uMsg)
	{
	case WM_CREATE:
		SetTimer(hwnd, idTimer = 1, 8, NULL);
		RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE);
		//SetWindowPos(hTaskBar, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		
		break;
	case WM_TIMER:
	{
		RedrawWindow(hwnd, NULL, NULL, RDW_UPDATENOW | RDW_INVALIDATE);
		break;
	}
	case WM_DESTROY:
	{
		PostQuitMessage(0);
		return 0;
	}
	case WM_PAINT:
	{
		

		hdc = BeginPaint(hwnd, &ps);
		if(!blur)
			SetWindowBlur(hTaskBar, 6);
		else
			SetWindowBlur(hTaskBar, 3);

		
		GetWindowRect(hTaskBar, &tr);
		//SetWindowPos(hTaskBar, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);


		HBRUSH brush = CreateSolidBrush(current);
		SelectObject(hdc, brush);
		
		if (gradient) {
			GradientFill(hdc, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_H);
		}
		else {
			FillRect(hdc, &ps.rcPaint, brush);
		}
		DeleteObject(brush);

		SetWindowPos(hwnd, hTaskBar, tr.left, tr.top, tr.right, tr.bottom, 0);
		
		
		EndPaint(hwnd, &ps);
		
		break;
	}
	return 0;

	}
	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
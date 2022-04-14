// rainbow taskbar program thing
// by ad2017!!!

#define _CRT_SECURE_NO_WARNINGS
#pragma comment(lib, "Msimg32.lib")
#pragma comment(lib, "Gdi32.lib")
#pragma comment(lib, "Winmm.lib")
#pragma comment(lib, "Shcore.lib")

#include <windows.h>
#include <synchapi.h>
#include <Uxtheme.h>
#include <tchar.h>
#include <stdio.h>
#include <stdlib.h>
#include <wingdi.h>
#include <windowsx.h>
#include <timeapi.h>
#include <shobjidl_core.h>
#include <combaseapi.h>
#include <shlwapi.h>
#include <ShellScalingApi.h>


#include "config.h"
#include "accent.h"
#include "gui.h"

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK WindowProc2(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
void BorderRadius(LPVOID a);

DWORD WINAPI Thrd(void* data);

HANDLE penzi;
HANDLE corner, corner2;

UINT old_border_radius = 0;
COLORREF current = 0;
HWND winhwnd;
HWND winhwnd2;

HWND hTaskBar;
HWND hTaskBar2;

void RnbTskWnd();

void NewConf(rtcfg* nw) {
	TerminateThread(corner, 0);
	if (hTaskBar2) TerminateThread(corner2, 0);
	SetAccentColor(GetAccentColor());
	old_border_radius = 0;

	memcpy(rcfg, nw, 2048 * sizeof(rtcfg_step));
	TerminateThread(penzi, 0);
	penzi = CreateThread(NULL,
		0,
		Thrd,
		rcfg,
		0,
		NULL);

	corner = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)BorderRadius, hTaskBar, 0, 0);
	if (hTaskBar2) {
		corner2 = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)BorderRadius, hTaskBar2, 0, 0);
	}
}

rtcfg* ConfigParser(rtcfg* cfg) {
	char* appdata = getenv("APPDATA");
	char* confpath[512];
	sprintf(confpath, "%s\\rnbconf.txt", appdata);
	FILE* fconfig = fopen(confpath, "rb");
	char* _ln = malloc(512);
	int i = 0;
	cfg->fci = -1;
	while (fgets(_ln, 511, fconfig)) {
		if (_ln[0] == '#' || _ln[0] == '\r' || _ln[0] == '\n') continue;

		rtcfg_step step = { 0 };
		step.r = 1;
		sscanf(_ln, "%c %i %i %i %i %s %i %i %i %i %i %i %127s", &step.prefix, &step.time, &step.r, &step.g, &step.b, &step.effect, &step.effect_1, &step.effect_2, &step.effect_3, &step.effect_4, &step.effect_5, &step.effect_6, &step.effect_7);
		memcpy(step.full_line, _ln, 512);
		if (step.prefix == 'c' && cfg->fci < 0) {
			cfg->fci = 1;
			current = RGB(step.r, step.g, step.b);
		}
		cfg->steps[i++] = step;
	}
	cfg->len = i;
	free(_ln);
	fclose(fconfig);

	return cfg;
}


void RnbTskWnd() {
	DWORD dwStyle;

	hTaskBar = FindWindow(_T("Shell_TrayWnd"), 0);

	RECT tr;
	RECT tr2;
	LPCTSTR CLASS_NAME = L"RnbTskWnd";

	WNDCLASS wc = { 0 };

	wc.lpfnWndProc = WindowProc;
	wc.hInstance = GetModuleHandleA(NULL);
	wc.lpszClassName = CLASS_NAME;

	LPCTSTR CLASS_NAME2 = L"RnbTskWnd2";

	WNDCLASS wc2 = { 0 };

	wc2.lpfnWndProc = WindowProc2;
	wc2.hInstance = GetModuleHandleA(NULL);
	wc2.lpszClassName = CLASS_NAME2;

	RegisterClass(&wc);
	RegisterClass(&wc2);
	GetWindowRect(hTaskBar, &tr);
	winhwnd = CreateWindowEx(
		WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT,
		CLASS_NAME,
		L"RainbowTaskbar",
		WS_POPUP | WS_VISIBLE | CS_DROPSHADOW,
		tr.left,
		tr.top,
		tr.right - tr.left,
		tr.bottom - tr.top,
		0,
		NULL,
		GetModuleHandleA(NULL),
		NULL
	);
	//SetParent(cw, hTaskBar);
	if (hTaskBar2 = FindWindow(_T("Shell_SecondaryTrayWnd"), 0)) {
		GetWindowRect(hTaskBar2, &tr2);
		winhwnd2 = CreateWindowEx(
			WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT,
			CLASS_NAME2,
			L"RainbowTaskbar",
			WS_POPUP | WS_VISIBLE | CS_DROPSHADOW,
			0,
			0,
			100,
			100,
			0,
			NULL,
			GetModuleHandleA(NULL),
			NULL
		);
		//SetParent(cw, hTaskBar2);
	}
	penzi = CreateThread(NULL,
		0,
		Thrd,
		rcfg,
		0,
		NULL);

}

void OnDestroy() {
	TerminateThread(corner, 0);
	if (hTaskBar2) TerminateThread(corner2, 0);
	TerminateThread(penzi, 0);

	Sleep(50);

	SetAccentColor(GetAccentColor());
	SetLayeredWindowAttributes(hTaskBar, 0, 255, LWA_ALPHA);
	if (hTaskBar2) SetLayeredWindowAttributes(hTaskBar2, 0, 255, LWA_ALPHA);
	RedrawWindow(hTaskBar, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);

	RECT r;
	GetWindowRect(hTaskBar, &r);
	HRGN rgn = CreateRectRgn(0, 0, 99999, 99999);
	SetWindowRgn(hTaskBar, rgn, TRUE);
	DeleteObject(rgn);

	if (hTaskBar2) {
		GetWindowRect(hTaskBar2, &r);
		rgn = CreateRectRgn(0, 0, 99999, 99999);
		SetWindowRgn(hTaskBar2, rgn, TRUE);
		DeleteObject(rgn);
	}
	exit(0);
}

HWND hwd_;
int main(int argc, char* argv[]) {
	FreeConsole();
	LoadUX();


	TIMECAPS tc;
	timeGetDevCaps(&tc, sizeof(tc));
	timeBeginPeriod(tc.wPeriodMin);
	srand(time(NULL));

	if (hwd_ = FindWindow(L"RnbTskWnd", NULL)) {
		DWORD procid;
		GetWindowThreadProcessId(hwd_, &procid);
		HANDLE proc = OpenProcess(PROCESS_TERMINATE, FALSE, procid);
		DestroyWindow(hwd_);
		TerminateProcess(proc, 0);
	}

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
	STARTUP = ERROR_SUCCESS == RegGetValue(HKEY_CURRENT_USER, _T("Software\\Microsoft\\Windows\\CurrentVersion\\Run"),
		_T("RainbowTaskbar"), RRF_RT_REG_SZ, 0, 0, 0);


	// begin
	rcfg = malloc(2048 * sizeof(rtcfg_step));
	ConfigParser(rcfg);

	CreateThread(0, 0, (LPTHREAD_START_ROUTINE)RnbTskGUI, GetModuleHandle(NULL), 0, 0);

	//SetAccentColor(0x00000000);
	//current = GetAccentColor();
	if (rcfg->fci < 0) {
		current = GetAccentColor();
	}


	RnbTskWnd();



	/*
	NOTIFYICONDATA nidApp = { 0 };

	nidApp.cbSize = sizeof(NOTIFYICONDATA);
	nidApp.hWnd = (HWND)cw;
	nidApp.uID = 0xDEADBEEF;
	nidApp.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	nidApp.hIcon = hMainIcon;
	nidApp.uCallbackMessage = WM_USER_SHELLICON;
	LoadString(GetModuleHandleA(NULL), IDS_APPTOOLTIP, nidApp.szTip, MAX_LOADSTRING);
	*/

	MSG msg = { 0 };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}


	timeEndPeriod(tc.wPeriodMin);
	free(rcfg);
	return 0;
}



BOOL blur, blur2 = FALSE;
HDC hdc;
TRIVERTEX vertex[2] = { 0 };
GRADIENT_RECT gRect;
BOOL gradient = FALSE;
BOOL img = FALSE;
BOOL randomize;
RECT tr;
RECT imagepos;
BYTE imagealpha;
RECT imagesize;
HBITMAP image;
BITMAP _image;
UINT border_radius = 0;
char* imgloc;
int rnba, tska = 0;



DWORD WINAPI Thrd(void* data) {
	srand(GetTickCount() + getpid());

	imgloc = malloc(256);
	rtcfg* cfg = (rtcfg*)rcfg;
	vertex[0].Blue = (current >> 16 & 0xFF) << 8;
	vertex[0].Green = (current >> 8 & 0xFF) << 8;
	vertex[0].Red = (current & 0xFF) << 8;
	vertex[1].Blue = (current >> 16 & 0xFF) << 8;
	vertex[1].Green = (current >> 8 & 0xFF) << 8;
	vertex[1].Red = (current & 0xFF) << 8;
	while (1) {
		int slept = 0;
		for (int i = 0; i < cfg->len; i++) {
			rtcfg_step step = cfg->steps[i];
			if (step.prefix == 'c') {
				slept = 1;
				if (!strcmp(step.effect, "none")) {
					if (randomize) {
						randomize = FALSE;
						current = RGB(rand() % 256, rand() % 256, rand() % 256);
					}
					else {
						current = RGB(step.r, step.g, step.b);
					}
					Sleep(step.time);

				}
				else if (!strcmp(step.effect, "fade")) {
					int j = 1;
					COLORREF nw;
					if (randomize) {
						randomize = FALSE;
						nw = RGB(rand() % 256, rand() % 256, rand() % 256);
					}
					else {
						nw = RGB(step.r, step.g, step.b);
					}
					COLORREF last = current;
					while (j++ < (step.effect_1 / 20)) {
						current = interp(last, nw, (double)j / (step.effect_1 / 20), step.effect_2);
						Sleep(step.effect_1 / (step.effect_1 / 20));
					}
					Sleep(step.time);
				}
				else if (!strcmp(step.effect, "grad")) {
					UINT r1, g1, b1, r2, g2, b2;
					if (randomize) {
						randomize = FALSE;
						r1 = (rand() % 256) << 8;
						g1 = (rand() % 256) << 8;
						b1 = (rand() % 256) << 8;
						r2 = (rand() % 256) << 8;
						g2 = (rand() % 256) << 8;
						b2 = (rand() % 256) << 8;
					}
					else {
						r1 = step.r << 8;
						g1 = step.g << 8;
						b1 = step.b << 8;
						r2 = step.effect_1 << 8;
						g2 = step.effect_2 << 8;
						b2 = step.effect_3 << 8;
					}
					GetWindowRect(hTaskBar, &tr);

					vertex[0].x = 0;
					vertex[0].y = 0;
					vertex[0].Red = r1;
					vertex[0].Green = g1;
					vertex[0].Blue = b1;
					vertex[0].Alpha = 0xFFFF;

					vertex[1].x = tr.right - tr.left;
					vertex[1].y = tr.bottom - tr.top;
					vertex[1].Red = r2;
					vertex[1].Green = g2;
					vertex[1].Blue = b2;
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
					COLORREF nw1, nw2;
					if (randomize) {
						randomize = FALSE;
						nw1 = RGB(rand() % 256, rand() % 256, rand() % 256);
						nw2 = RGB(rand() % 256, rand() % 256, rand() % 256);
					}
					else {
						nw1 = RGB(step.r, step.g, step.b);
						nw2 = RGB(step.effect_1, step.effect_2, step.effect_3);
					}
					int j = 1;

					while (j++ < (step.effect_4 / 20)) {
						COLORREF interp1 = interp(c1, nw1, (double)j / (step.effect_4 / 20), step.effect_5);
						COLOR* rgb1 = (COLOR*)&interp1;
						vertex[0].x = 0;
						vertex[0].y = 0;
						vertex[0].Red = rgb1->R << 8;
						vertex[0].Green = rgb1->G << 8;
						vertex[0].Blue = rgb1->B << 8;
						vertex[0].Alpha = 0;

						COLORREF interp2 = interp(c2, nw2, (double)j / (step.effect_4 / 20), step.effect_5);
						COLOR* rgb2 = (COLOR*)&interp2;
						vertex[1].Red = rgb2->R << 8;
						vertex[1].Green = rgb2->G << 8;
						vertex[1].Blue = rgb2->B << 8;
						vertex[1].Alpha = 0;
						gRect.UpperLeft = 1;
						gRect.LowerRight = 0;
						gradient = TRUE;
						Sleep(step.effect_4 / (step.effect_4 / 20));
					}
					Sleep(step.time);
					gradient = FALSE;
				}
				else {
					slept = 0;
				}

			}
			else if (step.prefix == 't') {
				switch (step.time) {
				case 1: {
					if (tska == step.r) break;
					DWORD dwStyle = GetWindowLong(hTaskBar, GWL_EXSTYLE);
					SetWindowLong(hTaskBar, GWL_EXSTYLE, dwStyle | WS_EX_LAYERED);
					SetLayeredWindowAttributes(hTaskBar, 0, step.r, LWA_ALPHA);
					SetWindowPos(hTaskBar, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					if (hTaskBar2) {
						DWORD dwStyle = GetWindowLong(hTaskBar2, GWL_EXSTYLE);
						SetWindowLong(hTaskBar2, GWL_EXSTYLE, dwStyle | WS_EX_LAYERED);
						SetLayeredWindowAttributes(hTaskBar2, 0, step.r, LWA_ALPHA);
						SetWindowPos(hTaskBar2, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}

					tska = step.r;
					break;
				}
				case 2: {
					if (rnba == step.r) break;
					DWORD dwStyle = GetWindowLong(winhwnd, GWL_EXSTYLE);
					ShowWindow(winhwnd, FALSE);
					SetWindowLong(winhwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
					SetLayeredWindowAttributes(winhwnd, 0, step.r, LWA_ALPHA);
					ShowWindow(winhwnd, TRUE);
					SetWindowPos(winhwnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					if (winhwnd2) {
						DWORD dwStyle = GetWindowLong(winhwnd2, GWL_EXSTYLE);
						ShowWindow(winhwnd, FALSE);
						SetWindowLong(winhwnd2, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
						SetLayeredWindowAttributes(winhwnd2, 0, step.r, LWA_ALPHA);
						ShowWindow(winhwnd, TRUE);
						SetWindowPos(winhwnd2, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}
					rnba = step.r;
					break;
				}
				case 3: {
					if (rnba == step.r && tska == step.r) break;
					DWORD dwStyle = GetWindowLong(hTaskBar, GWL_EXSTYLE);
					SetWindowLong(hTaskBar, GWL_EXSTYLE, dwStyle | WS_EX_LAYERED);
					SetLayeredWindowAttributes(hTaskBar, 0, step.r, LWA_ALPHA);
					SetWindowPos(hTaskBar, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					if (hTaskBar2) {
						DWORD dwStyle = GetWindowLong(hTaskBar2, GWL_EXSTYLE);
						SetWindowLong(hTaskBar2, GWL_EXSTYLE, dwStyle | WS_EX_LAYERED);
						SetLayeredWindowAttributes(hTaskBar2, 0, step.r, LWA_ALPHA);
						SetWindowPos(hTaskBar2, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}

					dwStyle = GetWindowLong(winhwnd, GWL_EXSTYLE);
					ShowWindow(winhwnd, FALSE);
					SetWindowLong(winhwnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
					SetLayeredWindowAttributes(winhwnd, 0, step.r, LWA_ALPHA);
					ShowWindow(winhwnd, TRUE);
					SetWindowPos(winhwnd, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					if (winhwnd2) {
						DWORD dwStyle = GetWindowLong(winhwnd2, GWL_EXSTYLE);
						ShowWindow(winhwnd2, FALSE);
						SetWindowLong(winhwnd2, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
						SetLayeredWindowAttributes(winhwnd2, 0, step.r, LWA_ALPHA);
						ShowWindow(winhwnd2, TRUE);
						SetWindowPos(winhwnd2, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
					}

					rnba = step.r;
					tska = step.r;
					break;

				}
				case 4: {
					blur = step.r;
					break;
				}
				}


			}
			else if (step.prefix == 'w') {
				Sleep(step.time);
				slept = 1;
			}
			else if (step.prefix == 'i') {
				if (!strcmp(step.effect, imgloc)) continue;
				imgloc = step.effect;
				image = (HBITMAP)LoadImageA(
					NULL,
					step.effect,
					IMAGE_BITMAP,
					step.effect_2,
					step.effect_3,
					LR_LOADFROMFILE
				);
				GetObject(image, sizeof(BITMAP), &_image);
				RECT _imagepos = { step.time, step.r, step.g, step.b };
				imagepos = _imagepos; // ???
				imagealpha = step.effect_1 == 0 ? 255 : (step.effect_1 <= -1 ? 0 : step.effect_1);
				img = TRUE;
			}
			else if (step.prefix == 'r') {
				randomize = TRUE;
			}
			else if (step.prefix == 'b') {
				border_radius = step.time;
			}
		}

		if (!slept)
			Sleep(INFINITE); // bad cpu usage fix
	}
	free(imgloc);

}

float getScalingFactor()
{
	HDC desktop = GetDC(NULL);
	int real = GetDeviceCaps(desktop, VERTRES);
	int fals = GetDeviceCaps(desktop, DESKTOPVERTRES);

	float scale = (float)fals / (float)real;
	ReleaseDC(NULL, desktop);
	return scale;
}

BOOL autohide() {
	APPBARDATA ap;
	ap.cbSize = sizeof(APPBARDATA);

	UINT state = SHAppBarMessage(ABM_GETSTATE, &ap);

	return state & ABS_AUTOHIDE;
}

void BorderRadius(LPVOID a) {
	HWND tsk = (HWND)a;
	HWND hwnd = tsk == hTaskBar ? winhwnd : winhwnd2;

	while (1) {
		if (border_radius) {
			if (border_radius != old_border_radius) {
				SetAccentColor(GetAccentColor());
			}
			old_border_radius = border_radius;

			float scale = getScalingFactor();

			RECT r;
			GetWindowRect(tsk, &r);
			UINT w = (r.right - r.left) * scale;
			UINT h = (r.bottom - r.top) * scale;

			HRGN rgn = 0;
			if (!autohide() && hTaskBar2) {

				HRGN rgn = CreateRectRgn(0, 0, 0, 0);
				HRGN secrgn = CreateRectRgn(0, 0, 0, 0);
				if (tsk == hTaskBar2) {
					HRGN rgn2 = CreateRectRgn(0, 0, w / 2, h + 1);
					HRGN rgn1 = CreateRoundRectRgn(w / 3, 0, w + 1, h + 1, border_radius, border_radius);
					CombineRgn(rgn, rgn1, rgn2, RGN_OR);
					SetWindowRgn(tsk, rgn, TRUE);

					DeleteObject(rgn1);
					DeleteObject(rgn2);
				}
				else {
					HRGN rgn1 = CreateRoundRectRgn(0, 0, w + 1, h + 1, border_radius, border_radius);
					HRGN rgn2 = CreateRectRgn(w / 2, 0, w + 1, h + 1);
					CombineRgn(rgn, rgn1, rgn2, RGN_OR);
					SetWindowRgn(tsk, rgn, TRUE);

					DeleteObject(rgn1);
					DeleteObject(rgn2);
				}


				DeleteObject(secrgn);
			}
			else {
				rgn = CreateRoundRectRgn(0, 0, w + 1, h + 1, border_radius, border_radius);
				SetWindowRgn(tsk, rgn, TRUE);
			}


			DeleteObject(rgn);
			rgn = CreateRectRgn(0, 0, 0, 0);
			GetWindowRgn(tsk, rgn);
			SetWindowRgn(hwnd, rgn, FALSE);
			DeleteObject(rgn);
		}
		Sleep(150);
	}

}

#define TIMER_MOVE 1
#define TIMER_REPAINT 2

PAINTSTRUCT ps;
#define ARGB_ABGR(a,r,g,b) ( (r) | ( (g)<<8 ) | ( (b) <<16) | ( (a) <<24 ) ) 
LRESULT CALLBACK WndPrc1(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam, HWND which) {
	HWND tsk = which;
	switch (uMsg)
	{
	case WM_CREATE:
	{
		SetTimer(hwnd, TIMER_MOVE, 2, NULL);
		SetTimer(hwnd, TIMER_REPAINT, 10, NULL);
		if (which == hTaskBar) {
			corner = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)BorderRadius, which, 0, 0);
		}
		else if (which == hTaskBar2) {
			corner2 = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)BorderRadius, which, 0, 0);
		}
		break;
	}
	case WM_TIMER:
	{
		if (wParam == TIMER_MOVE) {
			RECT tr;
			GetWindowRect(tsk, &tr);

			SetWindowPos(hwnd, tsk, tr.left, tr.top, tr.right - tr.left, tr.bottom - tr.top, SWP_NOACTIVATE | SWP_SHOWWINDOW);
			//MoveWindow(hwnd, tsk, tr.left, tr.top, tr.right, tr.bottom, TRUE);
		}
		else {
			InvalidateRect(hwnd, NULL, 0);
			UpdateWindow(hwnd);
		}
		break;
	}
	case WM_PAINT:
	{
		hdc = BeginPaint(hwnd, &ps);
		GetWindowRect(tsk, &tr);

		int WIDTH = tr.right - tr.left, HEIGHT = tr.bottom - tr.top;

		HDC buffer = CreateCompatibleDC(hdc);
		HBITMAP Membitmap = CreateCompatibleBitmap(hdc, WIDTH, HEIGHT);
		SelectObject(buffer, Membitmap);
		if (!blur)
			SetWindowBlur(tsk, 2);
		else
			SetWindowBlur(tsk, 2);



		//SetWindowPos(hTaskBar, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);


		HBRUSH brush = CreateSolidBrush(current);
		SelectObject(hdc, brush);


		if (gradient) {
			vertex[1].x = WIDTH;
			vertex[1].y = HEIGHT;
			GradientFill(buffer, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_H);
		}
		else {
			FillRect(buffer, &ps.rcPaint, brush);
		}
		if (img) {
			HDC hdcMem = CreateCompatibleDC(buffer);
			SelectBitmap(hdcMem, image);
			int r = imagepos.right > 0 ? imagepos.right : WIDTH;
			int b = imagepos.bottom > 0 ? imagepos.bottom : HEIGHT;
			BLENDFUNCTION bf = { 0, 0, imagealpha, 0 };

			AlphaBlend(
				buffer, imagepos.left, imagepos.top, r, b,
				hdcMem, 0, 0, _image.bmWidth, _image.bmHeight, bf);
			DeleteDC(hdcMem);
		}
		BitBlt(hdc, 0, 0, WIDTH, HEIGHT, buffer, 0, 0, SRCCOPY);
		DeleteObject(Membitmap);
		DeleteDC(buffer);
		DeleteDC(hdc);
		DeleteObject(brush);




		EndPaint(hwnd, &ps);
		break;
	}
	case WM_CLOSE:
	{
		PostQuitMessage(0);
		return 0;
	}
	case WM_DESTROY:
	{
		PostQuitMessage(0);
		return 0;
	}
	return 0;

	}

}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	winhwnd = hwnd;
	WndPrc1(hwnd, uMsg, wParam, lParam, hTaskBar);
	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

LRESULT CALLBACK WindowProc2(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	winhwnd2 = hwnd;
	WndPrc1(hwnd, uMsg, wParam, lParam, hTaskBar2);
	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

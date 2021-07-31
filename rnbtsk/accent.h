#pragma once
#include <windows.h>
#define C16(a,b) a << 8 | b

typedef struct {
	COLORREF color1;
	COLORREF color2;
} COLOR_PREF;

typedef struct
{
	int nAccentState;
	int nFlags;
	int nColor;
	int nAnimationId;
} ACCENTPOLICY;
typedef struct
{
	int nAttribute;
	PVOID pData;
	ULONG ulDataSize;
} WINCOMPATTRDATA;
typedef struct {
	BYTE R;
	BYTE G;
	BYTE B;
	BYTE IGNORED;
} COLOR;

void SetWindowBlur(HWND hWnd, DWORD appearance);
void LoadUX();
COLORREF GetAccentColor();
void SetAccentColor(COLORREF color);
COLORREF clerp(unsigned int color1, unsigned int color2, double fraction);
#pragma once
#include <windows.h>
#define C16(a,b) a << 8 | b
#define ARGB(a,r,g,b) (COLORREF)( (b) | ( (g)<<8 ) | ( (r) <<16) | ( (a) <<24 ) ) 

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

enum INTERP {
	LINEAR,

	SINE,
	CUBIC,
	EXPONENTIAL,
	BACK
};

HRESULT(WINAPI* GetUserColorPreference)(COLOR_PREF* cpPreference, BOOL fForceReload);
HRESULT(WINAPI* SetUserColorPreference)(COLOR_PREF* cpPreference, BOOL fForceCommit);
BOOL(WINAPI* SetWindowCompositionAttribute)(HWND, WINCOMPATTRDATA*);
BOOL(WINAPI* GetWindowCompositionAttribute)(HWND, WINCOMPATTRDATA*);
ACCENTPOLICY GetWindowAccentPolicy(HWND hWnd);
void SetWindowBlur(HWND hWnd, DWORD appearance);
void SetWindowABlur(HWND hWnd, DWORD att, COLORREF color);
void LoadUX();
COLORREF GetAccentColor();
void SetAccentColor(COLORREF color);
COLORREF interp(COLORREF color1, COLORREF color2, double fraction, enum INTERP which);
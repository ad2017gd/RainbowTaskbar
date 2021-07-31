#include <windows.h>
#include <dwmapi.h>
#include "accent.h"

// mess of undocumented functions

static HRESULT(WINAPI* GetUserColorPreference)(COLOR_PREF* cpPreference, BOOL fForceReload);
static HRESULT(WINAPI* SetUserColorPreference)(COLOR_PREF* cpPreference, BOOL fForceCommit);
static BOOL(WINAPI* SetWindowCompositionAttribute)(HWND, WINCOMPATTRDATA*);
void LoadUX() {
	HMODULE hUxTheme = LoadLibrary(L"uxtheme.dll");
	HMODULE user32 = LoadLibrary(L"user32.dll");

	GetUserColorPreference = GetProcAddress(hUxTheme, "GetUserColorPreference");
	SetUserColorPreference = GetProcAddress(hUxTheme, (LPCSTR)122);
	SetWindowCompositionAttribute = GetProcAddress(user32, "SetWindowCompositionAttribute");
}


COLORREF GetAccentColor()
{
	HRESULT h;

	COLOR_PREF cp;
	h = GetUserColorPreference(&cp, FALSE);

	return cp.color2 & 0x00FFFFFF;
}


void SetWindowBlur(HWND hWnd, DWORD appearance)
{
	if (SetWindowCompositionAttribute)
	{
		ACCENTPOLICY policy = { 0 };
		policy.nAccentState = appearance ? appearance : 2;
		policy.nAnimationId = 2;
		policy.nColor = RGB(0, 0, 0);
		policy.nFlags = 0;
		WINCOMPATTRDATA data = { 19, &policy, sizeof(ACCENTPOLICY) };
		SetWindowCompositionAttribute(hWnd, &data);
	}

}

void SetAccentColor(COLORREF color)
{
	HRESULT h;

	COLOR_PREF cp;
	h = GetUserColorPreference(&cp, FALSE);

	color &= 0x00FFFFFF;

	cp.color2 = color;
	h = SetUserColorPreference(&cp, TRUE);
}

COLORREF clerp(unsigned int color1, unsigned int color2, double fraction)
{
	COLOR* c1 = (COLOR*)&color1;
	COLOR* c2 = (COLOR*)&color2;

	return RGB(((c2->R - c1->R) * fraction + c1->R),
		((c2->G - c1->G) * fraction + c1->G),
		((c2->B - c1->B) * fraction + c1->B));
}
float lncv(float v0, float v0min, float v0max, float v1min, float v1max) {
	return (((v0 - v0min) * (v1max - v1min)) / (v0max - v0min)) + v1min;
}

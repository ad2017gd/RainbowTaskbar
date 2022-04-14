#include <windows.h>
#include <dwmapi.h>
#include <math.h>
#include <VersionHelpers.h>
#include "accent.h"

// mess of undocumented functions
#pragma comment(lib, "Dwmapi.lib")

void LoadUX() {
	HMODULE hUxTheme = LoadLibrary(L"uxtheme.dll");
	HMODULE user32 = LoadLibrary(L"user32.dll");

	GetUserColorPreference = GetProcAddress(hUxTheme, "GetUserColorPreference");
	SetUserColorPreference = GetProcAddress(hUxTheme, (LPCSTR)122);
	SetWindowCompositionAttribute = GetProcAddress(user32, "SetWindowCompositionAttribute");
	GetWindowCompositionAttribute = GetProcAddress(user32, "GetWindowCompositionAttribute");
}


COLORREF GetAccentColor()
{
	if (!IsWindows10OrGreater()) return 0;
	if (GetUserColorPreference) {
		HRESULT h;

		COLOR_PREF cp;
		h = GetUserColorPreference(&cp, FALSE);

		return cp.color2 & 0x00FFFFFF;
	}
}

void SetWindowBlur(HWND hWnd, DWORD appearance)
{
	if (SetWindowCompositionAttribute)
	{
		ACCENTPOLICY policy = { 0 };
		policy.nAccentState = appearance ? appearance : 2;
		policy.nAnimationId = 2;
		policy.nColor = RGB(255,255,255);
		policy.nFlags = 0;
		WINCOMPATTRDATA data = { 19, &policy, sizeof(ACCENTPOLICY) };
		SetWindowCompositionAttribute(hWnd, &data);
	}

}
void SetWindowABlur(HWND hWnd, DWORD att, COLORREF color)
{
	if (SetWindowCompositionAttribute)
	{
		ACCENTPOLICY policy = { 0 };
		policy.nAccentState = att;
		policy.nAnimationId = 2;
		policy.nColor = color;
		policy.nFlags = 2;
		WINCOMPATTRDATA data = { 19, &policy, sizeof(ACCENTPOLICY) };
		SetWindowCompositionAttribute(hWnd, &data);
	}

}
ACCENTPOLICY GetWindowAccentPolicy(HWND hWnd) {
	if (GetWindowCompositionAttribute) {
		ACCENTPOLICY policy = { 0 };
		WINCOMPATTRDATA data = { 19, &policy, sizeof(ACCENTPOLICY) };
		GetWindowCompositionAttribute(hWnd, &data);
		return policy;
	}
	
}

void SetAccentColor(COLORREF color)
{
	if (!IsWindows10OrGreater()) return 0;
	if (SetUserColorPreference) {
		HRESULT h;

		COLOR_PREF cp;
		h = GetUserColorPreference(&cp, FALSE);

		color &= 0x00FFFFFF;

		cp.color2 = color;
		cp.color1 = color;
		h = SetUserColorPreference(&cp, TRUE);
	}
	
}

UINT clamp(double d, double min, double max) {
	UINT t = d < min ? min : d;
	return t > max ? max : t;
}
COLORREF clerp(COLORREF color1, COLORREF color2, double fraction)
{
	COLOR* c1 = (COLOR*)&color1;
	COLOR* c2 = (COLOR*)&color2;
	BYTE r, g, b;
	r = clamp( ((c2->R - c1->R) * fraction + c1->R), 0, 255);
	g = clamp( ((c2->G - c1->G) * fraction + c1->G), 0, 255);
	b = clamp( ((c2->B - c1->B) * fraction + c1->B), 0, 255);

	return RGB(r,
		g,
		b);
}

// reference @ https://easings.net/
double easecubic(double x){
	return x < 0.5 ? 4 * x * x * x : 1 - pow(-2 * x + 2, 3) / 2;
}
double easeback(double x) {
	const double c1 = 1.70158;
	const double c2 = c1 * 1.525;

	return x < 0.5
		? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
		: (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
}
double easesine(double x) {
	return -(cos(3.14159 /*PI*/ * x) - 1) / 2;
}
double easeexp(double x) {
	return x == 0
		? 0
		: x == 1
		? 1
		: x < 0.5 ? pow(2, 20 * x - 10) / 2
		: (2 - pow(2, -20 * x + 10)) / 2;
}
//

COLORREF funcinterp(COLORREF color1, COLORREF color2, double fraction, double (*f)(double))
{
	double easefrac = f(fraction);
	return clerp(color1, color2, easefrac);
}


// Abstraction
COLORREF interp(COLORREF color1, COLORREF color2, double fraction, enum INTERP which) {
	switch (which) {
	case LINEAR:
	{
		return clerp(color1, color2, fraction);
	}
	case SINE:
	{
		return funcinterp(color1, color2, fraction, easesine);
	}
	case CUBIC:
	{
		return funcinterp(color1, color2, fraction, easecubic);
	}
	case EXPONENTIAL:
	{
		return funcinterp(color1, color2, fraction, easeexp);
	}
	case BACK:
	{
		return funcinterp(color1, color2, fraction, easeback);
	}
	default :
	{
		return clerp(color1, color2, fraction);
	}
	}
}
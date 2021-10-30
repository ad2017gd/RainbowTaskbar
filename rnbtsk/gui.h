#pragma once

#include <windows.h>
#include <dwmapi.h>
#include <stdlib.h>
#include <stdio.h>
#include <windowsx.h>
#include <shellapi.h>
#include <strsafe.h>
#include <tchar.h>
#include <shlwapi.h>

#include "map.h"
#include "config.h"

void RnbTskGUI(HINSTANCE hInstance);

#define WM_NEWTEXT 0xAD2017
#define WC_TEXT L"TEXT"
#define WM_USER_SHELLICON WM_APP+1

typedef struct {
    int x;
    int y;
    int len;
    BOOL enabled;
    LPWSTR text;
} CONTROL_TEXT;

rtcfg* cfg;

HICON hMainIcon;
HFONT guiFont;
MapRef(controls, HWND);
MapRef(texts, CONTROL_TEXT);
BOOL XY;



int index;
int tindex;
int r, g, b;
int which;
HBRUSH transparent;
HTREEITEM selected;
rtcfg_step selstep;
int selint;
DWORD rgbCurr;
DWORD rgbCurr2;

LRESULT CALLBACK GUIProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

void Config_ParseLine(char* ln);
void ConfView_Init(HWND confview);
void Cycle();
void InitWnd();
#pragma once

#include <windows.h>
#include "gui.h"


HWND Control(LPCSTR name, LPTSTR class, LPTSTR text, DWORD style, int x, int y, int w, int h);
void EnableControl(HWND ctrl, BOOL enable);
void TextModify(int index, LPTSTR text);
void EnableText(int index, BOOL enable);
void Control_HideAll();

void ConfView_Init(HWND confview);
int  ConfView_Index(HTREEITEM item);
void ConfView_OnSelect();

void Remove_OnClick();
void Add_OnClick();

void ComboBox1_OnModify();
void ComboBox2_OnModify();
void ComboBox3_OnModify();
void ColorPick1_OnClick();
void ColorPick2_OnClick();
void Slider1_OnModify();
void PickFile_OnClick();
void Edit0_OnModify();
void Edit1_OnModify();
void Edit2_OnModify();
void Edit3_OnModify();

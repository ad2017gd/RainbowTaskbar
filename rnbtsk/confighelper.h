#pragma once

#include "config.h"

void Config_ParseLine(char* ln);
void Config_MakeLn(int idx);
void Config_MakeFullLine();
void Config_Modify(char* what, void* to);
void Config_Pop(int idx);
void Config_Insert(int idx, rtcfg_step what);
void Config_Apply();
void Control_HideAll();
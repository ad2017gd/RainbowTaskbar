#pragma once

// convenience lib


char* __SWITCH_STR__;
wchar_t* __SWITCH_WSTR__;

#define switchs(str) __SWITCH_STR__ = str; if (0)
#define cases(str) } else if (strcmp(__SWITCH_STR__, str) == 0) {switch(1) { case 1:

#define switchsw(wstr) __SWITCH_WSTR__ = wstr; if (0)
#define casesw(wstr) } else if (wcscmp(__SWITCH_WSTR__, wstr) == 0) {switch(1) { case 1:

#define breaks }
#define defaults } else {switch(1) { case 1:
#define defaultsw defaults
#define breaksw breaks
//

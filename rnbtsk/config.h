#pragma once

// hacky way of geting the default config in
// who cares anyway
#ifndef _RNBDEFCONFIG
const char* _RNBDEFCONFIG = "\n\
# Detailed config documentation at https://github.com/ad2017gd/RainbowTaskbar#usage \n\
# format:\n\
# c (ms) (r) (g) (b) (effect) (... effect options) - change taskbar color\n\
# t (1 = taskbar; 2 = rainbowtaskbar; 3 = both; 4 = blur taskbar, ignores alpha) (alpha, 0 - 255) - set transparency\n\
# w (ms) - wait N ms\n\
# i (x) (y) (width, full = 0) (height, full = 0) (full path, without spaces) (alpha, opaque = 255) (image crop width, optional) (image crop height, optional)\n\
\n\
# effects:\n\
# none - solid color\n\
# fade (ms) (steps, optional) - fade solid color\n\
# grad (r2) (g2) (b2) - gradient\n\
# fgrd (r2) (g2) (b2) (ms) (steps, optional) - fade gradient\n\
\n\
# fade : do not use a high amount of steps!the color interpolation function is not optimized,\n\
#and the Win32 Sleep function is inaccurate at low values\n\
\n\
t 4\n\
t 2 200\n\
\n\
c 1 255 0 0 fgrd 255 154 0 500\n\
c 1 255 154 0 fgrd 208 222 33 500\n\
c 1 208 222 33 fgrd 79 220 74 500\n\
c 1 79 220 74 fgrd 63 218 216 500\n\
c 1 63 218 216 fgrd 47 201 226 500\n\
c 1 47 201 226 fgrd 28 127 238 500\n\
c 1 28 127 238 fgrd 95 21 242 500\n\
c 1 95 21 242 fgrd 186 12 248 500\n\
c 1 186 12 248 fgrd 251 7 217 500\n\
c 1 251 7 217 fgrd 255 0 0 500";
#endif
typedef struct {
	char prefix;
	int time;
	BYTE r;
	BYTE g;
	BYTE b;
	char effect[256];
	int effect_1;
	int effect_2;
	int effect_3;
	int effect_4;
	int effect_5;
	int effect_6;
	char effect_7[128];
} rtcfg_step;

typedef struct {
	int len;
	rtcfg_step steps[];
} rtcfg;

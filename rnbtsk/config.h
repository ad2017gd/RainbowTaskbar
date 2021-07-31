#pragma once

// hacky way of geting the default config in
// who cares anyway
#ifndef _RNBDEFCONFIG
const char* _RNBDEFCONFIG = "\n\
# format:\n\
# c (ms) (r) (g) (b) (effect) (... effect options) - change taskbar color\n\
# t (1 = taskbar, 2 = rainbowtaskbar, 3 = both, 4 = blur taskbar, ignores alpha) (alpha, 0 - 255) - set transparency\n\
# d (ms) - wait N ms\n\
\n\
# effects:\n\
# none - solid color\n\
# fade (ms) (steps) - fade solid color\n\
# grad (r2) (g2) (b2) - gradient\n\
\n\
# fade : do not use a high amount of steps!the color interpolation function is not optimized,\n\
# and the Win32 Sleep function is inaccurate at low values\n\
\n\
t 4\n\
t 1 200\n\
c 1000 140 0 185 fgrd 0 189 208 5000 100\n\
c 1000 0 189 208 fgrd 140 0 185 5000 100";
#endif
typedef struct {
	char prefix;
	int time;
	BYTE r;
	BYTE g;
	BYTE b;
	char effect[5];
	int effect_1;
	int effect_2;
	int effect_3;
	int effect_4;
	int effect_5;
	int effect_6;
	char effect_7[8];
} rtcfg_step;

typedef struct {
	int len;
	rtcfg_step steps[];
} rtcfg;
#pragma once

// hacky way of geting the default config in
// who cares anyway
#ifndef _RNBDEFCONFIG
const char* _RNBDEFCONFIG = "\n\
# format:\n\
# c (ms) (r) (g) (b) (effect) (... effect options)\n\
\n\
# effects:\n\
# none (0) (0) (0)\n\
# fade (ms) (steps) (0)\n\
# do not use a high amount of steps! the color interpolation function is not optimized at all\n\
\n\
c 10 28 118 201 fade 4000 50 0\n\
c 10 214 28 235 fade 4000 50 0\n\
c 10 80 28 235 fade 1000 50 0";
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
} rtcfg_step;

typedef struct {
	int len;
	rtcfg_step steps[];
} rtcfg;
#include "gui.h"
#include "controls.h"
#include "config.h"
#include "accent.h"
#include "switchs.h"
#include "confighelper.h"

void Config_ParseLine(char* ln) {
    sscanf(ln, "%c %i %i %i %i %s %i %i %i %i %i %i %127s", &selstep.prefix, &selstep.time, &selstep.r, &selstep.g, &selstep.b, &selstep.effect, &selstep.effect_1, &selstep.effect_2, &selstep.effect_3, &selstep.effect_4, &selstep.effect_5, &selstep.effect_6, &selstep.effect_7);
}

void Config_MakeLn(int idx) {
    rtcfg_step* ss = &cfg->steps[idx];
    switch (ss->prefix) {
    case 't':
        sprintf(ss->full_line, "t %i %i", ss->time, ss->r);
        break;
    case 'w':
        sprintf(ss->full_line, "w %i", ss->time);
        break;
    case 'i':
        sprintf(ss->full_line, "i %i %i %i %i %s %i %i %i", ss->time, ss->r, ss->g, ss->b, ss->effect, ss->effect_1, ss->effect_2, ss->effect_3);
        break;
    case 'c':
        switchs(ss->effect) {
            cases("none") {
                sprintf(ss->full_line, "c %i %i %i %i none", ss->time, ss->r, ss->g, ss->b);
                breaks;
            }
            cases("fade") {
                sprintf(ss->full_line, "c %i %i %i %i fade %i %i", ss->time, ss->r, ss->g, ss->b, ss->effect_1, ss->effect_2);
                breaks;
            }
            cases("grad") {
                sprintf(ss->full_line, "c %i %i %i %i grad %i %i %i", ss->time, ss->r, ss->g, ss->b, ss->effect_1, ss->effect_2, ss->effect_3);
                breaks;
            }
            cases("fgrd") {
                sprintf(ss->full_line, "c %i %i %i %i fgrd %i %i %i %i %i", ss->time, ss->r, ss->g, ss->b, ss->effect_1, ss->effect_2, ss->effect_3, ss->effect_4, ss->effect_5);
                breaks;
            }
            defaults{
                sprintf(ss->full_line, "c %i %i %i %i none", ss->time, ss->r, ss->g, ss->b);
                breaks;
            }

        }
        break;
    case 'r':
        sprintf(ss->full_line, "r");
        break;
    case 'b':
        sprintf(ss->full_line, "b %i", ss->time);
        break;
    }
}

void Config_MakeFullLine() {
    switch (selstep.prefix) {
    case 't':
        sprintf(selstep.full_line, "t %i %i", selstep.time, selstep.r);
        break;
    case 'w':
        sprintf(selstep.full_line, "w %i", selstep.time);
        break;
    case 'i':
        sprintf(selstep.full_line, "i %i %i %i %i %s %i %i %i", selstep.time, selstep.r, selstep.g, selstep.b, selstep.effect, selstep.effect_1, selstep.effect_2, selstep.effect_3);
        break;
    case 'c':
        switchs(selstep.effect) {
            cases("none") {
                sprintf(selstep.full_line, "c %i %i %i %i none", selstep.time, selstep.r, selstep.g, selstep.b);
                breaks;
            }
            cases("fade") {
                sprintf(selstep.full_line, "c %i %i %i %i fade %i %i", selstep.time, selstep.r, selstep.g, selstep.b, selstep.effect_1, selstep.effect_2);
                breaks;
            }
            cases("grad") {
                sprintf(selstep.full_line, "c %i %i %i %i grad %i %i %i", selstep.time, selstep.r, selstep.g, selstep.b, selstep.effect_1, selstep.effect_2, selstep.effect_3);
                breaks;
            }
            cases("fgrd") {
                sprintf(selstep.full_line, "c %i %i %i %i fgrd %i %i %i %i %i", selstep.time, selstep.r, selstep.g, selstep.b, selstep.effect_1, selstep.effect_2, selstep.effect_3, selstep.effect_4, selstep.effect_5);
                breaks;
            }
            defaults{
                sprintf(selstep.full_line, "c %i %i %i %i none", selstep.time, selstep.r, selstep.g, selstep.b);
                breaks;
            }

        }
        break;
    case 'r':
        sprintf(selstep.full_line, "r");
        break;
    case 'b':
        sprintf(selstep.full_line, "b %i", selstep.time);
        break;
    }
    HWND confview;
    MapGet(controls, "treeview_conf", confview);

    int idx = ConfView_Index(selected);
    cfg->steps[idx] = selstep;

    LPWSTR str = malloc(513 * sizeof(wchar_t));
    mbstowcs(str, selstep.full_line, 512);

    TVITEM item;
    item.hItem = selected;
    item.mask = TVIF_TEXT;
    item.pszText = str;
    item.cchTextMax = 512;

    TreeView_SetItem(confview, &item);
    free(str);

}

void Config_Modify(char* what, void* to) {
    switchs(what) {
        cases("rgb") {

            COLOR* color = (COLOR*)to;
            selstep.r = color->R;
            selstep.g = color->G;
            selstep.b = color->B;

            Config_MakeFullLine();
            breaks;
        }
        cases("type") {
            char* c = (char*)to;
            memset(&selstep, 0, sizeof(rtcfg_step));
            selstep.prefix = *c;
            Config_MakeFullLine();
            breaks;
        }
        cases("time") {
            int* c = (int*)to;
            selstep.time = *c;
            Config_MakeFullLine();
            breaks;
        }
        cases("r") {
            int* r = (int*)to;
            selstep.r = *r;
            Config_MakeFullLine();
            breaks;
        }
        cases("g") {
            int* g = (int*)to;
            selstep.g = *g;
            Config_MakeFullLine();
            breaks;
        }
        cases("b") {
            int* b = (int*)to;
            selstep.b = *b;
            Config_MakeFullLine();
            breaks;
        }
        cases("effect") {
            strcpy(selstep.effect, (char*)to);
            Config_MakeFullLine();
            breaks;
        }
        cases("effect1") {
            int* b = (int*)to;
            selstep.effect_1 = *b;
            Config_MakeFullLine();
            breaks;
        }
        cases("effect2") {
            int* b = (int*)to;
            selstep.effect_2 = *b;
            Config_MakeFullLine();
            breaks;
        }
        cases("rgb2") {
            COLOR* color = (COLOR*)to;
            selstep.effect_1 = color->R;
            selstep.effect_2 = color->G;
            selstep.effect_3 = color->B;

            Config_MakeFullLine();
            breaks;
        }
        cases("effect4") {
            int* b = (int*)to;
            selstep.effect_4 = *b;
            Config_MakeFullLine();
            breaks;
        }
        cases("effect5") {
            int* b = (int*)to;
            selstep.effect_5 = *b;
            Config_MakeFullLine();
            breaks;
        }
        cases("xycord1") {
            POINT* xycord1 = (POINT*)to;
            selstep.time = xycord1->x;
            selstep.r = xycord1->y;

            Config_MakeFullLine();
            breaks;
        }
        cases("xycord2") {
            POINT* xycord2 = (POINT*)to;
            selstep.g = xycord2->x;
            selstep.b = xycord2->y;

            Config_MakeFullLine();
            breaks;
        }
        cases("xycord3") { // unused
            POINT* xycord3 = (POINT*)to;
            selstep.effect_2 = xycord3->x;
            selstep.effect_3 = xycord3->y;

            Config_MakeFullLine();
            breaks;
        }

    }
    cfg->steps[selint] = selstep;

}

void Config_Pop(int idx) {
    if (idx < cfg->len - 1) {
        int move = idx;
        memset(&cfg->steps[idx], 0, sizeof(rtcfg_step));
        while (move < cfg->len) {
            memcpy(&cfg->steps[move], &cfg->steps[move + 1], sizeof(rtcfg_step));
            move++;
        }
    }
    else {
        memset(&cfg->steps[idx], 0, sizeof(rtcfg_step));
    }
    cfg->len--;
}
void Config_Insert(int idx, rtcfg_step what) {
    if (idx == 0 && cfg->len == 0) {
        memcpy(&cfg->steps[idx], &what, sizeof(rtcfg_step));
        cfg->len += 1;
        return;
    }
    if (idx < cfg->len) {


        for (int i = cfg->len; i >= idx; i--)
            memcpy(&cfg->steps[i], &cfg->steps[i - 1], sizeof(rtcfg_step));

        memcpy(&cfg->steps[idx], &what, sizeof(rtcfg_step));
    }
    else {
        memcpy(&cfg->steps[idx], &what, sizeof(rtcfg_step));
    }
    cfg->len++;
}

void Config_Apply() {
    char* appdata = getenv("APPDATA");
    char* confpath[512];
    sprintf(confpath, "%s\\rnbconf.txt", appdata);
    FILE* conf = fopen(confpath, "w");
    fprintf(conf, "# RNBTSK CFG\n\n");
    fclose(conf);
    conf = fopen(confpath, "a");
    for (int i = 0; i < cfg->len; i++) {
        Config_MakeLn(i);
        fprintf(conf, "%s\n", cfg->steps[i].full_line);
    }
    fclose(conf);

    NewConf(cfg);
}
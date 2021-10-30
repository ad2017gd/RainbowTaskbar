#include <windows.h>
#include "controls.h"
#include "config.h"
#include "accent.h"
#include "gui.h"
#include "switchs.h"
#include "confighelper.h"
#include "others.h"

// Basic control functions

HWND Control(LPCSTR name, LPTSTR class, LPTSTR text, DWORD style, int x, int y, int w, int h) {
    if (!wcscmp(class, WC_TEXT)) { // fuck win32 text 
        CONTROL_TEXT txt = { 0 };
        txt.len = wcslen(text);
        txt.text = text;
        txt.x = x;
        txt.y = y;
        txt.enabled = TRUE;

        MapAdd(texts, name, txt);

        return tindex++;
        SendMessage(mainn, WM_NEWTEXT, NULL, NULL);
    }
    HWND a = CreateWindowExW(!strcmp(class, WC_EDIT) || !strcmp(class, WC_TREEVIEW) ? WS_EX_CLIENTEDGE : 0L,
        class,
        text,
        style | WS_VISIBLE | WS_CHILD,
        x,
        y,
        w,
        h,
        mainn,
        index++,
        GetModuleHandleA(NULL),
        NULL);
    if (!wcscmp(class, WC_BUTTON)) {
        /* dude if anyone can fix this i'd be so fucking thankful :SOB:
        SetWindowLong(a, GWL_EXSTYLE, GetWindowLong(a, GWL_EXSTYLE) | WS_EX_LAYERED);
        SetLayeredWindowAttributes(a, 0xf0f0f0, 255, LWA_COLORKEY | LWA_ALPHA);
        EnableControl(a, FALSE);
        EnableControl(a, TRUE);
        */
    }
    SendMessage(a, WM_SETFONT, (LPARAM)guiFont, TRUE);
    MapAdd(controls, name, a);
    return a;
}

void EnableControl(HWND ctrl, BOOL enable) {
    EnableWindow(ctrl, enable);
    ShowWindow(ctrl, enable);
}

void TextModify(int index, LPTSTR text) {
    MapArray(texts)[index].text = text;
    MapArray(texts)[index].len = wcslen(text);

    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
}
void EnableText(int index, BOOL enable) {
    MapArray(texts)[index].enabled = enable;
    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
}

void Control_HideAll() {
    HWND ctrl;
    CONTROL_TEXT unused;
    int idx;
    char* key;
    MapIterate(controls, key, ctrl, idx,
        if (idx > 2 && strcmp(key, "button_apply")) EnableControl(ctrl, FALSE);
    );
    MapIterate(texts, key, unused, idx,
        if (idx > 0) EnableText(idx, FALSE);
    );

    HWND app;
    MapGet(controls, "button_apply", app);
    EnableControl(app, TRUE);

    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);

}

// Individual control events

BOOL deleting = FALSE; // please suggest any other fix if it exists, but i couldnt think of anything better

void ConfView_Init(HWND confview) {
    deleting = TRUE;
    TreeView_DeleteItem(confview, NULL /* all */);
    deleting = FALSE;
    memset(&selstep, 0, sizeof(selstep));

    for (int i = 0; i < cfg->len; i++) {
        rtcfg_step cur = cfg->steps[i];

        TVINSERTSTRUCT tvis = { 0 };
        tvis.hParent = NULL;
        tvis.hInsertAfter = TVI_LAST;
        tvis.item.mask = TVIF_TEXT;
        char* line = malloc(strlen(cur.full_line) + 1);
        strcpy(line, cur.full_line);
        if (cur.full_line[strlen(cur.full_line) - 1] == '\n') {
            memcpy(line, cur.full_line, strlen(cur.full_line) - 3);
            line[strlen(cur.full_line) - 2] = '\0';
        }

        wchar_t* buf = malloc(513 * sizeof(wchar_t));
        size_t discard;
        mbstowcs(buf, line, 512);
        tvis.item.pszText = buf;
        tvis.item.cchTextMax = 512;
        TreeView_InsertItem(confview, &tvis);

        free(buf);
        free(line);
    }
}

int ConfView_Index(HTREEITEM item) {
    int index = 0;
    BOOL found = FALSE;
    HWND confview;
    MapGet(controls, "treeview_conf", confview);

    HTREEITEM root = TreeView_GetNextItem(confview, NULL, TVGN_ROOT);
    HTREEITEM cur = root;

    while (!found) {
        if (cur == item) { found = TRUE; break; }
        cur = TreeView_GetNextItem(confview, cur, TVGN_NEXT);
        if (cur == NULL) break;
        index++;
    }
    return found ? index : -1;
}



void ConfView_OnSelect() {
    if (deleting) return;

    HWND ctrl;
    MapGet(controls, "treeview_conf", ctrl);

    selected = TreeView_GetSelection(ctrl);
    TCHAR buffer[512];

    TVITEM item;
    item.hItem = selected;
    item.mask = TVIF_TEXT;
    item.cchTextMax = 512;
    item.pszText = buffer;
    TreeView_GetItem(ctrl, &item);
    selint = ConfView_Index(selected);

    CHAR buffer2[512];

    wcstombs(buffer2, buffer, 512);

    Config_ParseLine(buffer2);

    rgbCurr = RGB(selstep.r, selstep.g, selstep.b);

    Control_HideAll();
    HWND picker;
    MapGet(controls, "combobox_type", picker);
    EnableControl(picker, TRUE);

    int idx;
    MapGetIdx(texts, "label_helper0", idx);
    TextModify(idx, L"- Unknown");
    EnableText(idx, TRUE);

    XY = FALSE;

    switch (selstep.prefix) {
    case 'r':
    {
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Randomize next colors");

        ComboBox_SelectString(picker, 0, L"Randomize");
        break;
    }
    case 'b':
    {
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Set taskbar border radius");

        ComboBox_SelectString(picker, 0, L"Border radius");

        HWND radius;
        MapGet(controls, "slider_radius", radius);

        SendMessage(radius, TBM_SETRANGEMIN, TRUE, 0);
        SendMessage(radius, TBM_SETRANGEMAX, TRUE, 64);
        SendMessage(radius, TBM_SETPOS, TRUE, selstep.time);

        MapGetIdx(texts, "label_helper1", idx);
        TextModify(idx, L"- Border radius in pixels");
        EnableText(idx, TRUE);

        EnableControl(radius, TRUE);

        break;
    }
    case 'w':
    {
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Sleep for n milliseconds");


        ComboBox_SelectString(picker, 0, L"Delay");
        MapGetIdx(texts, "label_helper1", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"ms - Time in milliseconds to wait for");

        HWND input;
        MapGet(controls, "edit_time", input);

        LPWSTR delay = malloc(17 * sizeof(WCHAR));
        wsprintfW(delay, L"%i", selstep.time);

        Edit_SetText(input, delay);
        free(delay);

        EnableControl(input, TRUE);
        break;
    }
    case 't':
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Set opacity or blur of taskbar");

        ComboBox_SelectString(picker, 0, L"Transparency");
        MapGetIdx(texts, "label_helper1", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Feature to modify");

        HWND input;
        MapGet(controls, "combobox_time", input);

        LPWSTR who = L"";
        if (selstep.time == 0) { who = L"Taskbar"; selstep.time = 1; }
        if (selstep.time == 2) who = L"RnbTsk";
        if (selstep.time == 1) who = L"Taskbar";
        if (selstep.time == 3) who = L"Both";
        if (selstep.time == 4) who = L"Blur";
        ComboBox_SelectString(input, 0, who);

        HWND slider;
        MapGet(controls, "slider_alpha", slider);
        EnableControl(slider, TRUE);

        MapGetIdx(texts, "label_helper2", idx);
        EnableText(idx, TRUE);
        TextModify(idx, selstep.time == 4 ? L"- Enable or disable blur" : L"- Set opacity");

        SendMessage(slider, TBM_SETRANGEMIN, TRUE, 0);
        SendMessage(slider, TBM_SETRANGEMAX, TRUE, selstep.time == 4 ? 1 : 255);
        SendMessage(slider, TBM_SETPOS, TRUE, selstep.r);


        EnableControl(input, TRUE);
        break;
    case 'c':
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Set color effect");

        ComboBox_SelectString(picker, 0, L"Color");
        MapGetIdx(texts, "label_helper1", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"ms - Effect time in milliseconds");

        HWND in1;
        MapGet(controls, "edit_time", in1);
        LPWSTR txt = malloc(17 * sizeof(wchar_t));
        wsprintf(txt, L"%i", selstep.time);
        Edit_SetText(in1, txt);
        free(txt);

        HWND BTN;
        MapGet(controls, "button_colorpick1", BTN);

        MapGetIdx(texts, "label_helper2", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Color 1 picker");

        HWND pick2;
        MapGet(controls, "combobox_effect", pick2);
        switchs(selstep.effect) {
            cases("none") {
                ComboBox_SelectString(pick2, 0, L"Solid");
                breaks;
            }
            cases("fade") {
                ComboBox_SelectString(pick2, 0, L"Fade in solid");
                LPWSTR amuz1 = malloc(17 * sizeof(WCHAR));
                swprintf(amuz1, 16, L"%i", selstep.effect_1);

                HWND ef1;
                MapGet(controls, "edit_effect1", ef1);

                Edit_SetText(ef1, amuz1);

                int idx1;
                MapGetIdx(texts, "label_helper4", idx1);

                TextModify(idx1, L"- Fade duration in milliseconds");

                EnableText(idx1, TRUE);
                EnableControl(ef1, TRUE);

                free(amuz1);

                breaks;
            }
            cases("grad") {
                ComboBox_SelectString(pick2, 0, L"Gradient");


                HWND pickc2;
                MapGet(controls, "button_colorpick2", pickc2);
                EnableControl(pickc2, TRUE);
                rgbCurr2 = RGB(selstep.effect_1, selstep.effect_2, selstep.effect_3);
                breaks;
            }
            cases("fgrd") {
                ComboBox_SelectString(pick2, 0, L"Fade in gradient");
                LPWSTR amuz1 = malloc(17 * sizeof(WCHAR));
                swprintf(amuz1, 16, L"%i", selstep.effect_4);


                HWND pickc2, ef1, fadefunc;
                MapGet(controls, "button_colorpick2", pickc2);
                MapGet(controls, "edit_effect2", ef1);
                MapGet(controls, "combobox_fadefunc2", fadefunc);

                Edit_SetText(ef1, amuz1);

                free(amuz1);
                int idx1, idx2, idx3;
                MapGetIdx(texts, "label_helper4", idx1);
                MapGetIdx(texts, "label_helper5", idx2);
                MapGetIdx(texts, "label_helper6", idx3);

                TextModify(idx1, L"- Color 2 picker");
                TextModify(idx2, L"- Fade duration in milliseconds");
                TextModify(idx3, L"- Fade function");

                EnableText(idx1, TRUE);
                EnableText(idx2, TRUE);
                EnableText(idx3, TRUE);

                EnableControl(pickc2, TRUE);
                EnableControl(ef1, TRUE);
                EnableControl(fadefunc, TRUE);
                LPWSTR which = 
                      selstep.effect_5 == LINEAR ? L"Linear"
                    : selstep.effect_5 == SINE ? L"Sine"
                    : selstep.effect_5 == CUBIC ? L"Cubic"
                    : selstep.effect_5 == EXPONENTIAL ? L"Exponential"
                    : selstep.effect_5 == BACK ? L"Back" : L"Linear";
                                       
                                       
                                                
                ComboBox_SelectString(fadefunc, 0, which);
                rgbCurr2 = RGB(selstep.effect_1, selstep.effect_2, selstep.effect_3);
                breaks;
            }
        }
        MapGetIdx(texts, "label_helper3", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Color effect");

        EnableControl(in1, TRUE);
        EnableControl(BTN, TRUE);
        EnableControl(pick2, TRUE);
        break;
    case 'i':
    {
        MapGetIdx(texts, "label_helper0", idx);
        TextModify(idx, L"- Display bitmap image");


        ComboBox_SelectString(picker, 0, L"Image");

        MapGetIdx(texts, "label_helper1", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- XY coordinates for image");
        XY = TRUE;

        HWND input;
        MapGet(controls, "edit_time", input);

        LPWSTR xy1 = malloc(33 * sizeof(WCHAR));
        wsprintfW(xy1, L"%ix%i", selstep.time, selstep.r);

        Edit_SetText(input, xy1);
        free(xy1);
        EnableControl(input, TRUE);

        MapGetIdx(texts, "label_helper2", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Size on taskbar. Leave empty for full");

        HWND input2;
        MapGet(controls, "edit_r", input2);

        LPWSTR xy2 = malloc(33 * sizeof(WCHAR));
        wsprintfW(xy2, L"%ix%i", selstep.g, selstep.b);

        Edit_SetText(input2, xy2);
        free(xy2);
        EnableControl(input2, TRUE);

        HWND pickbut;
        MapGet(controls, "button_pickfile", pickbut);

        MapGetIdx(texts, "label_helper3", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Pick BMP image");

        EnableControl(pickbut, TRUE);

        HWND alpha;
        MapGet(controls, "edit_effect1", alpha);

        LPWSTR aph = malloc(33 * sizeof(WCHAR));
        wsprintfW(aph, L"%i", selstep.effect_1);

        Edit_SetText(alpha, aph);
        free(aph);
        MapGetIdx(texts, "label_helper4", idx);
        EnableText(idx, TRUE);
        TextModify(idx, L"- Image alpha (used to mix with color effects)");

        EnableControl(alpha, TRUE);



        break;
    }
    default:
        EnableWindow(picker, FALSE);
    }

    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
}



void Remove_OnClick() {
    if (cfg->len <= 0) {
        cfg->len = 0;
        return;
    }
    if (ConfView_Index(selected) == -1) return;



    Config_Pop(selint);

    HWND del;
    MapGet(controls, "treeview_conf", del);
    TreeView_DeleteItem(del, selected ? selected : NULL /* all */);

    if (cfg->len == 0) {
        HWND picker;
        MapGet(controls, "treeview_conf", picker);

        Control_HideAll();

    }
}

void Add_OnClick() {
    TVINSERTSTRUCT tvis = { 0 };
    tvis.hParent = NULL;
    tvis.hInsertAfter = selected ? selected : TVI_LAST;
    tvis.item.mask = TVIF_TEXT;
    tvis.item.pszText = L"c 100 0 0 0 none";
    tvis.item.cchTextMax = 512;
    HWND add;
    MapGet(controls, "treeview_conf", add);
    TreeView_InsertItem(add, &tvis);

    rtcfg_step buffer = { 0 };
    buffer.prefix = 'c';
    buffer.time = 100;
    buffer.r = 0;
    buffer.g = 0;
    buffer.b = 0;
    memcpy(buffer.effect, "none", 5);
    Config_Insert(selint + (cfg->len > 0), buffer);



}

void ComboBox1_OnModify() {
    char to = 'c';
    int to2 = 0;
    HWND cb;
    MapGet(controls, "combobox_type", cb);

    int index = ComboBox_GetCurSel(cb);
    wchar_t* sel = malloc(17 * sizeof(wchar_t));
    ComboBox_GetText(cb, sel, 16);


    // C T I D

    switchsw(sel) {
        casesw(L"Color") {
            to = 'c';
            breaks;
        }
        casesw(L"Transparency") {
            to = 't';
            to2 = 1;
            Config_Modify("time", &to2);
            breaks;
        }
        casesw(L"Image") {
            to = 'i';
            breaks;
        }
        casesw(L"Delay") {
            to = 'w';
            breaks;
        }
        casesw(L"Randomize") {
            to = 'r';
            breaks;
        }
        casesw(L"Border radius") {
            to = 'b';
            breaks;
        }
    }
    if (to == selstep.prefix) {
        free(sel);
        return;
    }

    Config_Modify("type", &to);
    ConfView_OnSelect();
    free(sel);
}
void ComboBox2_OnModify() {
    int to = 0;
    int to2 = 0;
    HWND cb;
    MapGet(controls, "combobox_time", cb);

    int index = ComboBox_GetCurSel(cb);
    wchar_t* sel = malloc(17 * sizeof(wchar_t));
    ComboBox_GetText(cb, sel, 16);

    switchsw(sel) {
        casesw(L"RnbTsk") {
            to = 2;
            to2 = 255;
            breaks;
        }
        casesw(L"Taskbar") {
            to = 1;
            to2 = 255;
            breaks;
        }
        casesw(L"Both") {
            to = 3;
            to2 = 255;
            breaks;
        }
        casesw(L"Blur") {
            to = 4;
            to2 = 1;
            breaks;
        }
    }

    HWND slider;
    MapGet(controls, "slider_alpha", slider);

    SendMessage(slider, TBM_SETRANGEMIN, TRUE, 0);
    SendMessage(slider, TBM_SETRANGEMAX, TRUE, to == 4 ? 1 : 255);
    SendMessage(slider, TBM_SETPOS, TRUE, to2);

    int idx;
    MapGetIdx(texts, "label_helper2", idx);
    EnableText(idx, TRUE);
    TextModify(idx, to == 4 ? L"- Enable or disable blur" : L"- Set opacity");

    Config_Modify("time", &to);
    Config_Modify("r", &to2);
    //ConfView_OnSelect();
    free(sel);
}

void ComboBox3_OnModify() {
    LPCSTR to = "none";
    int to1 = 100;
    enum INTERP to2 = LINEAR;
    COLOR to3 = { 0 };
    HWND cb;
    MapGet(controls, "combobox_effect", cb);

    int index = ComboBox_GetCurSel(cb);
    wchar_t* sel = malloc(25 * sizeof(wchar_t));
    ComboBox_GetText(cb, sel, 24);

    switchsw(sel) {
        casesw(L"Solid") {
            to = "none";
            breaks;
        }
        casesw(L"Fade in solid") {
            to = "fade";
            Config_Modify("effect1", &to1);
            Config_Modify("effect2", &to2);
            breaks;
        }
        casesw(L"Gradient") {
            to = "grad";
            Config_Modify("rgb2", &to3);
            breaks;
        }
        casesw(L"Fade in gradient") {
            to = "fgrd";
            Config_Modify("rgb2", &to3);
            Config_Modify("effect4", &to1);
            Config_Modify("effect5", &to2);
            breaks;
        }
        defaults{
            to = "none";
            breaks;
        }
    }

    Config_Modify("effect", to);
    ConfView_OnSelect();
    free(sel);
}


void ComboBox4_OnModify() {
    enum INTERP to = LINEAR;
    HWND cb;
    MapGet(controls, "combobox_fadefunc", cb);

    int index = ComboBox_GetCurSel(cb);
    wchar_t* sel = malloc(25 * sizeof(wchar_t));
    ComboBox_GetText(cb, sel, 24);

    switchsw(sel) {
        casesw(L"Linear") {
            to = LINEAR;
            breaks;
        }
        casesw(L"Sine") {
            to = SINE;
            breaks;
        }
        casesw(L"Cubic") {
            to = CUBIC;
            breaks;
        }
        casesw(L"Exponential") {
            to = EXPONENTIAL;
            breaks;
        }
        casesw(L"Back"){
            to = BACK;
            breaks;
        }
    }

    Config_Modify("effect2", &to);
    ConfView_OnSelect();
    free(sel);
}
void ComboBox5_OnModify() {
    enum INTERP to = LINEAR;
    HWND cb;
    MapGet(controls, "combobox_fadefunc2", cb);

    int index = ComboBox_GetCurSel(cb);
    wchar_t* sel = malloc(25 * sizeof(wchar_t));
    ComboBox_GetText(cb, sel, 24);

    switchsw(sel) {
        casesw(L"Linear") {
            to = LINEAR;
            breaks;
        }
        casesw(L"Sine") {
            to = SINE;
            breaks;
        }
        casesw(L"Cubic") {
            to = CUBIC;
            breaks;
        }
        casesw(L"Exponential") {
            to = EXPONENTIAL;
            breaks;
        }
        casesw(L"Back") {
            to = BACK;
            breaks;
        }
    }

    Config_Modify("effect5", &to);
    ConfView_OnSelect();
    free(sel);
}

void ColorPick1_OnClick() {
    rgbCurr = PickColor(0);
    Config_Modify("rgb", &rgbCurr);

    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
}
void ColorPick2_OnClick() {
    rgbCurr2 = PickColor(1);
    Config_Modify("rgb2", &rgbCurr2);

    RedrawWindow(mainn, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
}
void Slider1_OnModify() {
    if (selstep.prefix == 't') {
        HWND slider;
        MapGet(controls, "slider_alpha", slider);
        int val = SendMessage(slider, TBM_GETPOS, 0, 0);
        Config_Modify("r", &val);
    }
    else {
        HWND slider;
        MapGet(controls, "slider_radius", slider);
        int val = SendMessage(slider, TBM_GETPOS, 0, 0);
        Config_Modify("time", &val);
    }

}
void PickFile_OnClick() {
    LPWSTR path = malloc(257 * sizeof(WCHAR));
    FileDiag_Open(mainn, path);
    LPCSTR patha = malloc(257);

    wcstombs(patha, path, 256);

    Config_Modify("effect", patha);


    free(path);
    free(patha);

}

void Edit0_OnModify() {
    HWND edit;
    MapGet(controls, "edit_r", edit);

    LPWSTR here = malloc(33 * sizeof(wchar_t));
    Edit_GetText(edit, here, 32);
    POINT XYCORD2 = Point_FromString(here);

    Config_Modify("xycord2", &XYCORD2);

    free(here);
}
void Edit1_OnModify() {
    int val = 0;
    HWND edit;
    MapGet(controls, "edit_time", edit);

    LPWSTR here = malloc(33 * sizeof(wchar_t));
    if (!XY) {
        Edit_GetText(edit, here, 32);
        swscanf(here, L"%i", &val);

        Config_Modify("time", &val);
    }
    else {
        Edit_GetText(edit, here, 32);
        POINT XYCORD1 = Point_FromString(here);

        Config_Modify("xycord1", &XYCORD1);
    }
    free(here);
}
void Edit2_OnModify() {
    int val = 0;
    HWND edit;
    MapGet(controls, "edit_effect1", edit);

    LPWSTR here = malloc(33 * sizeof(wchar_t));

    Edit_GetText(edit, here, 32);
    swscanf(here, L"%i", &val);

    Config_Modify("effect1", &val);

    free(here);
}
void Edit3_OnModify() {
    int val = 0;
    HWND edit;
    MapGet(controls, "edit_effect2", edit);

    LPWSTR here = malloc(33 * sizeof(wchar_t));

    if (!XY) {
        Edit_GetText(edit, here, 32);
        swscanf(here, L"%i", &val);

        Config_Modify("effect4", &val);
    }
    else {
        Edit_GetText(edit, here, 32);
        POINT XYCORD1 = Point_FromString(here);

        Config_Modify("xycord3", &XYCORD1);
    }


    free(here);
}
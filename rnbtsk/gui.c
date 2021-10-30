//#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
//#pragma comment(linker,"/manifestcompatibility:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#pragma comment(lib, "shlwapi.lib")

#include "gui.h"
#include "accent.h"
#include "config.h"
#include "others.h"
#include "switchs.h"
#include "resource.h"
#include "confighelper.h"
#include "controls.h"

MapDef(controls, HWND);
MapDef(texts, CONTROL_TEXT);

COLORREF bgc = RGB(0x22, 0x22, 0x22);

DWORD CheckUpdate() {
    char data[1024];
    ExecCmd(L"cmd /c curl --help 1> nul 2> nul && echo %ERRORLEVEL%", data);
    if (!memcmp(data, "9009", 4)) {
        return;
    }
    ExecCmd(L"curl -m 10 -s https://ad2017.dev/rnbver.txt", data);
    HKEY hkey = NULL;
    RegOpenKey(HKEY_CURRENT_USER, _T("Software\\RainbowTaskbarNoUpdate"), &hkey);
    if (hkey != NULL) {
        return 1;
    }
    if (strcmp(data, RNBVER)) {
        // UPDATE!
        UINT ret = MessageBox(0, L"An update for RainbowTaskbar has been released. Do you want to update? (Click CANCEL if you don't want to be asked again)", L"Update", MB_YESNOCANCEL | MB_ICONINFORMATION);
        if (ret == IDYES) {
            wchar_t szFileName[MAX_PATH];
            GetModuleFileNameW(NULL, szFileName, MAX_PATH);
            wchar_t* filename = PathFindFileNameW(szFileName);
            wchar_t cmd[MAX_PATH * 3];
            memset(cmd, 0, sizeof(wchar_t) * MAX_PATH * 3);
            wcscat(cmd, L"cmd /c taskkill /f /im ");
            wcscat(cmd, filename);
            wcscat(cmd, L" && del /q /f ");
            wcscat(cmd, szFileName);
            wcscat(cmd, L" && curl -L ");
            if (RNBARCH == 64) {
                wcscat(cmd, L"https://github.com/ad2017gd/RainbowTaskbar/releases/latest/download/rnbtsk-x64.exe");
            }
            else {
                wcscat(cmd, L"https://github.com/ad2017gd/RainbowTaskbar/releases/latest/download/rnbtsk.exe");
            }
            wcscat(cmd, L" -o ");
            wcscat(cmd, szFileName);
            wcscat(cmd, L" && ");
            wcscat(cmd, szFileName);

            char nothing[1024];
            ExecCmd(cmd, nothing);
        }
        else if (ret == IDCANCEL) {
            RegCreateKey(HKEY_CURRENT_USER, _T("Software\\RainbowTaskbarNoUpdate"), &hkey);
        }
    }
    return 0;
}

void RnbTskGUI(HINSTANCE hInstance)
{
    MapInit(controls);
    MapInit(texts);
    CreateThread(0,0,(LPTHREAD_START_ROUTINE)CheckUpdate,0,0,0);

    transparent = CreateSolidBrush(bgc);

    Class(L"RnbTskGui", hInstance, GUIProc);

    mainn = CreateWindowExW(
        WS_EX_APPWINDOW | WS_EX_DLGMODALFRAME,
        L"RnbTskGui",
        L"",
        WS_OVERLAPPEDWINDOW ^ WS_THICKFRAME,
        CW_USEDEFAULT, CW_USEDEFAULT, 600, 620,

        NULL,
        NULL,
        hInstance,
        NULL
    );

    

    NONCLIENTMETRICS metrics;
    metrics.cbSize = sizeof(NONCLIENTMETRICS);
    SystemParametersInfo(SPI_GETNONCLIENTMETRICS, sizeof(NONCLIENTMETRICS),
        &metrics, 0);
    guiFont = CreateFont(20, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, DEFAULT_PITCH | FF_SWISS, L"Microsoft Sans Serif");
    SendMessage(mainn, WM_SETFONT, (LPARAM)guiFont, TRUE);

    InitWnd();


    //SetWindowABlur(mainn, 3, ARGB(64, 128, 0, 128));
    ShowWindow(mainn, FALSE);



    HICON hMainIcon = LoadImage(GetModuleHandle(NULL), MAKEINTRESOURCE(IDI_ICON1), IMAGE_ICON, 0, 0, 0);

    SendMessage(mainn, WM_SETICON, ICON_BIG, (LPARAM)hMainIcon);
    SendMessage(mainn, WM_SETICON, ICON_SMALL, (LPARAM)hMainIcon);

    NOTIFYICONDATA nidApp = { 0 };

    nidApp.cbSize = sizeof(NOTIFYICONDATA);
    nidApp.hWnd = (HWND)mainn;
    nidApp.uID = 0xAD2017;
    nidApp.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
    nidApp.hIcon = hMainIcon;
    nidApp.uCallbackMessage = WM_USER_SHELLICON;
    WCHAR wide[sizeof(RNBVER) * 2];
    mbstowcs(wide, RNBVER, sizeof(RNBVER) * 2);
    WCHAR real[32];
    wsprintfW(real, L"%s %s", L"RainbowTaskbar", wide);
    StringCchCopy(nidApp.szTip, ARRAYSIZE(nidApp.szTip), real);
    Shell_NotifyIcon(NIM_ADD, &nidApp);



    MSG msg = { 0 };
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    MapDelete(controls);
    MapDelete(texts);
    free(cfg);
    DeleteObject(guiFont);

    return 0;
}

void InitWnd() {
    int text1 = Control("label_rnbtsk", WC_TEXT, L"RainbowTaskbar Editor", 0, 10, 10, 0, 0);
    HWND confview = Control("treeview_conf", WC_TREEVIEW, L"Tree", 0, 10, 40, 500, 300);
    TreeView_SetBkColor(confview, RGB(20, 20, 20));
    TreeView_SetTextColor(confview, RGB(200, 200, 200));
    
    HWND remove = Control("button_remove", WC_BUTTON, L"x", 0, 520, 40, 25, 25);
    HWND add = Control("button_add", WC_BUTTON, L"+", 0, 520, 70, 25, 25);


    // common for every effect (duh)
    HWND typepicker = Control("combobox_type", WC_COMBOBOX, L"", CBS_DROPDOWNLIST, 10, 350, 150, 25);
    //ShowWindow(typepicker, FALSE);
    ComboBox_AddString(typepicker, L"Color");
    ComboBox_AddString(typepicker, L"Transparency");
    ComboBox_AddString(typepicker, L"Image");
    ComboBox_AddString(typepicker, L"Delay");
    ComboBox_AddString(typepicker, L"Randomize");
    ComboBox_AddString(typepicker, L"Border radius");

    int helper0 = Control("label_helper0", WC_TEXT, L"- Unknown", 0, 170, 355, 0, 0);

    HWND time = Control("edit_time", WC_EDIT, L"", 0, 10, 380, 100, 25);
    HWND alphapicker = Control("combobox_time", WC_COMBOBOX, L"", CBS_DROPDOWNLIST, 10, 380, 100, 25);
    ComboBox_AddString(alphapicker, L"Taskbar");
    ComboBox_AddString(alphapicker, L"RnbTsk");
    ComboBox_AddString(alphapicker, L"Both");
    ComboBox_AddString(alphapicker, L"Blur");
    HWND radius = Control("slider_radius", TRACKBAR_CLASS, L"Radius", TBS_HORZ, 10, 380, 100, 25);

    int helper1 = Control("label_helper1", WC_TEXT, L"HELPER TEXT HERE!", 0, 120, 385, 0, 0);
    

    // c - replaced by button ; (i - common ; t - replaced by slider)
    HWND r = Control("edit_r", WC_EDIT, L"", 0, 10, 410, 125, 25);
    HWND colorpick1 = Control("button_colorpick1", WC_BUTTON, L"Color 1", 0, 10, 410, 125, 25);
    HWND alpha = Control("slider_alpha", TRACKBAR_CLASS, L"Alpha", TBS_HORZ, 10, 410, 125, 25);

    int helper2 = Control("label_helper2", WC_TEXT, L"HELPER TEXT HERE!", 0, 140, 415, 0, 0);

    // c - drop down; i - choose file
    HWND effpicker = Control("combobox_effect", WC_COMBOBOX, L"", CBS_DROPDOWNLIST, 10, 440, 150, 25);
    ComboBox_AddString(effpicker, L"Solid");
    ComboBox_AddString(effpicker, L"Fade in solid");
    ComboBox_AddString(effpicker, L"Gradient");
    ComboBox_AddString(effpicker, L"Fade in gradient");
    HWND filepick = Control("button_pickfile", WC_BUTTON, L"Choose file", 0, 10, 440, 150, 25);
    int helper3 = Control("label_helper3", WC_TEXT, L"HELPER TEXT HERE!", 0, 170, 445, 0, 0);

    // c(fade),i - common ; c(grad,fgrd) - button;
    HWND eff1 = Control("edit_effect1", WC_EDIT, L"", 0, 10, 470, 100, 25);
    HWND colorpick2 = Control("button_colorpick2", WC_BUTTON, L"Color 2", 0, 10, 470, 100, 25);
    int helper4 = Control("label_helper4", WC_TEXT, L"HELPER TEXT HERE!", 0, 120, 475, 0, 0);

    // c(fgrd) - common; i - common,optional 
    HWND eff2 = Control("edit_effect2", WC_EDIT, L"", 0, 10, 500, 100, 25);
    HWND fadefunc = Control("combobox_fadefunc", WC_COMBOBOX, L"", CBS_DROPDOWNLIST, 10, 500, 100, 25);
    ComboBox_AddString(fadefunc, L"Linear");
    ComboBox_AddString(fadefunc, L"Sine");
    ComboBox_AddString(fadefunc, L"Cubic");
    ComboBox_AddString(fadefunc, L"Exponential");
    ComboBox_AddString(fadefunc, L"Back");
    int helper5 = Control("label_helper5", WC_TEXT, L"HELPER TEXT HERE!", 0, 115, 505, 0, 0);

    // c(fgrd) - picker
    HWND fadefunc2 = Control("combobox_fadefunc2", WC_COMBOBOX, L"", CBS_DROPDOWNLIST, 10, 530, 150, 25);
    ComboBox_AddString(fadefunc2, L"Linear");
    ComboBox_AddString(fadefunc2, L"Sine");
    ComboBox_AddString(fadefunc2, L"Cubic");
    ComboBox_AddString(fadefunc2, L"Exponential");
    ComboBox_AddString(fadefunc2, L"Back");
    int helper6 = Control("label_helper6", WC_TEXT, L"HELPER TEXT HERE!", 0, 165, 535, 0, 0);

    Control_HideAll();

    HWND apply = Control("button_apply", WC_BUTTON, L"Apply", BS_PUSHBUTTON, 480, 552, 100, 25);
    

    //
    cfg = malloc(2048 * sizeof(rtcfg_step));
    ConfigParser(cfg);

    ConfView_Init(confview);
}


#define MENUOPEN 0xAD20 - 1
#define MENU0 0xAD20 + 0
#define MENU1 0xAD20 + 1
#define MENU2 0xAD20 + 2
#define MENU3 0xAD20 + 3
#define MENU4 0xAD20 + 4
#define MENU5 0xAD20 + 5

#define SUB0 0xAE20 + 0
#define SUB1 0xAE20 + 1
#define SUB2 0xAE20 + 2
#define SUB3 0xAE20 + 3
#define SUB4 0xAE20 + 4

void Example(char* ex) {
    char* appdata = getenv("APPDATA");
    char* confpath[512];
    sprintf(confpath, "%s\\rnbconf.txt", appdata);
    FILE* fconfig = fopen(confpath, "w");
    fprintf(fconfig, "%s", ex);
    fclose(fconfig);
    ConfigParser(rcfg);
    ConfigParser(cfg);
    NewConf(rcfg);
    HWND confview;
    MapGet(controls, "treeview_conf", confview);
    ConfView_Init(confview);
}

LRESULT CALLBACK GUIProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    
    switch (uMsg)
    {
    case WM_USER_SHELLICON:
    {
        switch (LOWORD(lParam))
        {
        case WM_LBUTTONUP:
        {

            ShowWindow(hwnd, TRUE);
            BringWindowToTop(hwnd);
            SetForegroundWindow(hwnd);
            break;

        }
        case WM_RBUTTONUP:
        {
            HMENU menu = CreatePopupMenu();
            HMENU submenu = CreatePopupMenu();
            AppendMenu(submenu, 0, SUB0, L"Rainbow");
            AppendMenu(submenu, 0, SUB1, L"Pulse");
            AppendMenu(submenu, 0, SUB2, L"Ambient");
            AppendMenu(submenu, 0, SUB4, L"Randomize");
            AppendMenu(submenu, 0, SUB3, L"Static");

            AppendMenu(menu, 0, MENUOPEN, L"Open");
            AppendMenu(menu, 0, MENU0, L"Config file");
            AppendMenu(menu, MF_POPUP, submenu, L"Config examples");
            AppendMenu(menu, 0, MENU3, L"Project page");
            AppendMenu(menu, 0, MENU5, L"Donate");
            MENUITEMINFO real;
            HBITMAP bmp;
            bmp = LoadImage(GetModuleHandle(NULL), MAKEINTRESOURCE(IDB_BITMAP2), IMAGE_BITMAP, 16, 16, 0);
            
            real.cbSize = sizeof(MENUITEMINFO);
            real.fMask = MIIM_BITMAP;
            real.hbmpItem = bmp;
            SetMenuItemInfo(menu, MENU5, 0, &real);
            AppendMenu(menu, STARTUP ? MF_CHECKED : MF_UNCHECKED, MENU1, L"Run at startup");
            AppendMenu(menu, 0, MENU2, L"Exit");

            POINT p;
            GetCursorPos(&p);

            SetForegroundWindow(hwnd);
            TrackPopupMenu(menu, 0, p.x, p.y+8, 0, hwnd, 0);
            PostMessage(hwnd, WM_NULL, 0, 0);

            DestroyMenu(menu);
            break;
            
        }
        }
        break;
    }
    case WM_NEWTEXT: // custom message
        RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
        break;
    case WM_CREATE:
        
        SetActiveWindow(hwnd);
        break;
    case WM_NOTIFY:
        switch (((LPNMHDR)lParam)->code)
        {
        case TVN_SELCHANGED:
            ConfView_OnSelect((LPNMHDR)lParam);
            break;
        case TVN_KEYDOWN:
            switch (((NMTVKEYDOWN*)lParam)->wVKey) {
                case VK_DELETE:
                    Remove_OnClick();
                    break;
                case VK_INSERT:
                    Add_OnClick();
                    break;
            }
            break;
        }
        
        

        
        break;
    case WM_HSCROLL:
        Slider1_OnModify();
        break;
    case WM_COMMAND:
    {

        switch (wParam) {
            case MENUOPEN: {
                ShowWindow(hwnd, TRUE);
                BringWindowToTop(hwnd);
                SetForegroundWindow(hwnd);

                break;
            }
            case MENU0: // cfg file
            {
                system("start %appdata%\\rnbconf.txt");

                break;
            }
            case MENU1: // startup
            {
                STARTUP = !STARTUP;

                char* appdata = getenv("APPDATA");
                char* confpath[512];
                sprintf(confpath, "%s\\rnbconf.txt", appdata);

                FILE* fconfig = fopen(confpath, "r");
                TCHAR thisfile[MAX_PATH];
                GetModuleFileName(NULL, thisfile, MAX_PATH);

                HKEY hkey = NULL;
                RegCreateKey(HKEY_CURRENT_USER, _T("Software\\Microsoft\\Windows\\CurrentVersion\\Run"), &hkey);
                if (STARTUP) {
                    RegSetValueEx(hkey, _T("RainbowTaskbar"), 0, REG_SZ, thisfile, (_tcslen(thisfile) + 1) * sizeof(TCHAR));
                }
                else {
                    RegDeleteValue(hkey, _T("RainbowTaskbar"));
                }

                

                RegCloseKey(hkey);

                break;
            }
            case MENU2: // quit
            {
                OnDestroy();
                DestroyWindow(hwnd);
                break; // ?
            }
            case MENU3:
            {
                system("explorer \"https://github.com/ad2017gd/RainbowTaskbar\"");
                break;
            }
            case SUB0:
            {
                char example1[] = 
                    "t 4\n"
                    "t 2 200\n"
                    "\n"
                    "c 1 255 0 0 fgrd 255 154 0 500\n"
                    "c 1 255 154 0 fgrd 208 222 33 500\n"
                    "c 1 208 222 33 fgrd 79 220 74 500\n"
                    "c 1 79 220 74 fgrd 63 218 216 500\n"
                    "c 1 63 218 216 fgrd 47 201 226 500\n"
                    "c 1 47 201 226 fgrd 28 127 238 500\n"
                    "c 1 28 127 238 fgrd 95 21 242 500\n"
                    "c 1 95 21 242 fgrd 186 12 248 500\n"
                    "c 1 186 12 248 fgrd 251 7 217 500\n"
                    "c 1 251 7 217 fgrd 255 0 0 500";
                Example(example1);
                break;
            }
            case SUB1:
            {
                char example2[] =
                    "t 4\n"
                    "t 2 180\n"
                    "t 1 230\n"
                    "\n"
                    "c 750 255 0 180 fgrd 0 200 255 400\n"
                    "c 100 255 255 255 grad 255 255 255\n"
                    "c 750 0 200 255 fgrd 255 0 180 400\n"
                    "c 100 255 255 255 grad 255 255 255\n";
                Example(example2);
                break;
            }
            case SUB2:
            {
                char example3[] =
                    "t 4 1\n"
                    "t 3 201\n"
                    "c 1500 2 40 104 fgrd 0 165 253 4000\n"
                    "c 1500 0 165 253 fgrd 2 40 104 4000\n";
                Example(example3);
                break;
            }
            case SUB3:
            {
                char example4[] =
                    "t 4 0\n"
                    "t 3 224\n"
                    "c 99999999 30 120 219 none\n";
                Example(example4);
                break;
            }
            case SUB4:
            {
                char example5[] =
                    "t 4 1\n"
                    "t 3 201\n"
                    "r\n"
                    "c 250 0 0 0 fgrd 0 0 0 1000 4\n"
                    "r\n"
                    "c 250 0 0 0 fgrd 0 0 0 1000 4\n";
                Example(example5);
                break;
            }
            case MENU5:
            {
                system("explorer \"https://paypal.me/ad2k17\"");
                break;
            }
        }
        switch (HIWORD(wParam)) {

        case CBN_SELCHANGE:
        {
            char* key;
            MapFind(controls, lParam, key);

            switchs(key) {
                cases("combobox_type") {
                    ComboBox1_OnModify();
                    breaks;
                }
                cases("combobox_time") {
                    ComboBox2_OnModify();
                    breaks;
                }
                cases("combobox_effect") {
                    ComboBox3_OnModify();
                    breaks;
                }
                cases("combobox_fadefunc") {
                    ComboBox4_OnModify();
                    breaks;
                }
                cases("combobox_fadefunc2") {
                    ComboBox5_OnModify();
                    breaks;
                }
            }


            break;
        }
        case EN_UPDATE:
        {
            char* key;
            MapFind(controls, lParam, key);

            switchs(key) {
                cases("edit_r") {
                    Edit0_OnModify();
                    breaks;
                }
                cases("edit_time") {
                    Edit1_OnModify();
                    breaks;
                }
                cases("edit_effect1") {
                    Edit2_OnModify();
                    breaks;
                }
                cases("edit_effect2") {
                    Edit3_OnModify();
                    breaks;
                }
            }
            break;
        }
        case BN_CLICKED:
        {
            char* key;
            MapFind(controls, lParam, key);

            switchs(key) {
                cases("button_remove") {
                    Remove_OnClick();
                    breaks;
                }
                cases("button_add") {
                    Add_OnClick();
                    breaks;
                }
                cases("button_colorpick1") {
                    ColorPick1_OnClick();
                    breaks;
                }
                cases("button_colorpick2") {
                    ColorPick2_OnClick();
                    breaks;
                }
                cases("button_pickfile") {
                    PickFile_OnClick();
                    breaks;
                }
                cases("button_apply") {
                    Config_Apply();
                    breaks;
                }
            }
            break;
        }

        }
        
        break;
    }
    case WM_SIZE:
    {

        SendMessage(hwnd, WM_PRINT, (WPARAM)NULL, PRF_NONCLIENT);
        break;
    }
    case WM_SIZING:
        RedrawWindow(hwnd, NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW);
        break;
    case WM_CLOSE:
        ShowWindow(hwnd, FALSE);
        return 0;
    case WM_CTLCOLORSTATIC:
    {
        HDC hdcStatic = (HDC)wParam;
        SetBkColor(hdcStatic, bgc);
        SetTextColor(hdcStatic, RGB(255, 255, 255));
        SetDCBrushColor(hdcStatic, RGB(255,255,255));
        
        return (INT_PTR)transparent;
    }
    case WM_DESTROY:
        if (transparent) DeleteObject(transparent);

        NOTIFYICONDATA sh = { 0 };
        sh.uID = 0xad2017;
        sh.hWnd = hwnd;
        Shell_NotifyIcon(NIM_DELETE, &sh);

        HWND hwd_;
        if (hwd_ = FindWindow(L"RnbTskWnd", NULL)) {
            DWORD procid;
            GetWindowThreadProcessId(hwd_, &procid);
            HANDLE proc = OpenProcess(PROCESS_TERMINATE, FALSE, procid);
            DestroyWindow(hwd_);
            TerminateProcess(proc, 0);
        }
        PostQuitMessage(0);
        break;
    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);

        FillRect(hdc, &ps.rcPaint, transparent);
        //
        SelectObject(hdc, (LPARAM)guiFont);
        SetBkColor(hdc, bgc);
        SetTextColor(hdc, RGB(200, 200, 200));

        if (selstep.prefix == 'c') {
            HBRUSH c1 = CreateSolidBrush(RGB(selstep.r, selstep.g, selstep.b));
            switchs(selstep.effect) {
                cases("none") {
                    RECT rc;
                    rc = ps.rcPaint;
                    rc.bottom = 10;

                    FillRect(hdc, &rc, c1);
                        breaks;
                }
                cases("fade") {
                    RECT rc;
                    rc = ps.rcPaint;
                    rc.bottom = 10;

                    FillRect(hdc, &rc, c1);
                    breaks;
                }
                cases("grad") {

                    RECT rc;
                    rc = ps.rcPaint;
                    rc.bottom = 10;

                    TRIVERTEX        vertex[2];
                    GRADIENT_RECT    gRect = { 0 };
                    vertex[0].x = 0;
                    vertex[0].y = 0;
                    vertex[0].Red = selstep.r << 8;
                    vertex[0].Green = selstep.g << 8;
                    vertex[0].Blue = selstep.b << 8;
                    vertex[0].Alpha = 0xFFFF;

                    vertex[1].x = rc.right;
                    vertex[1].y = rc.bottom;
                    vertex[1].Red = selstep.effect_1 << 8;
                    vertex[1].Green = selstep.effect_2 << 8;
                    vertex[1].Blue = selstep.effect_3 << 8;
                    vertex[1].Alpha = 0xFFFF;

                    gRect.UpperLeft = 0;
                    gRect.LowerRight = 1;
                    GradientFill(hdc, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_H);
                    breaks;
                }
                cases("fgrd") {

                    RECT rc;
                    rc = ps.rcPaint;
                    rc.bottom = 10;

                    TRIVERTEX        vertex[2];
                    GRADIENT_RECT    gRect = { 0 };
                    vertex[0].x = 0;
                    vertex[0].y = 0;
                    vertex[0].Red = selstep.r << 8;
                    vertex[0].Green = selstep.g << 8;
                    vertex[0].Blue = selstep.b << 8;
                    vertex[0].Alpha = 0xFFFF;

                    vertex[1].x = rc.right;
                    vertex[1].y = rc.bottom;
                    vertex[1].Red = selstep.effect_1 << 8;
                    vertex[1].Green = selstep.effect_2 << 8;
                    vertex[1].Blue = selstep.effect_3 << 8;
                    vertex[1].Alpha = 0xFFFF;

                    gRect.UpperLeft = 0;
                    gRect.LowerRight = 1;
                    GradientFill(hdc, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_H);
                    breaks;
                }
            }
            DeleteObject(c1);
        }
        char* ky;
        CONTROL_TEXT txt;
        int idx;
        

        MapIterate(texts, ky, txt, idx, 
            if (!txt.enabled) continue;
            TextOut(hdc, txt.x, txt.y, txt.text, txt.len);
        );

        EndPaint(hwnd, &ps);
        DeleteDC(hdc);
    }
    return 0;

    }
    return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RainbowTaskbar.Helpers;

public class TaskbarHelper {


    public enum DWMWINDOWATTRIBUTE : uint {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation,
        DWMWA_PASSIVE_UPDATE_MODE,
        DWMWA_USE_HOSTBACKDROPBRUSH,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,
        DWMWA_BORDER_COLOR,
        DWMWA_CAPTION_COLOR,
        DWMWA_TEXT_COLOR,
        DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
        DWMWA_LAST

    }


    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

    public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

    public enum CombineRgnStyles {
        RGN_AND = 1,
        RGN_OR = 2,
        RGN_XOR = 3,
        RGN_DIFF = 4,
        RGN_COPY = 5,
        RGN_MIN = RGN_AND,
        RGN_MAX = RGN_COPY
    }

    public enum TaskbarStyle {
        Default,
        Blur,
        Transparent,
        ForceDefault // temporary value
    }

    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_LAYERED = 0x80000;
    public const int LWA_ALPHA = 0x2;
    public const int LWA_COLORKEY = 0x1;

    private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
    private const uint WINEVENT_OUTOFCONTEXT = 0;

    public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

    private static float scale = GetScalingFactor();
    private byte _alpha = 255;

    public bool autoHide = IsAutoHide();

    public bool first = false;
    public bool last = false;

    private IntPtr hhook;

    public IntPtr HWND = (IntPtr) 0;

    private bool layered;
    private int old_radius = -1;

    private WinEventDelegate procDelegate;
    public int Radius = 0;


    private IntPtr rgn = IntPtr.Zero;
    public bool Secondary;
    public TaskbarStyle Style = TaskbarStyle.Default;
    public Taskbar window = null;

    public TaskbarHelper(IntPtr hWnd, bool secondary = false) {
        HWND = hWnd;
        Secondary = secondary;

        SendMessage(HWND, WM_DWMCOMPOSITIONCHANGED, 1, null);
    }

    public event EventHandler TaskbarPositionChanged;

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);


    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
        int uFlags);

    [DllImportAttribute("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImportAttribute("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
        IntPtr lParam);

    public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Process process) {
        var handles = new List<IntPtr>();

        foreach (ProcessThread thread in process.Threads)
            EnumThreadWindows(thread.Id,
                (hWnd, lParam) => {
                    handles.Add(hWnd);
                    return true;
                }, IntPtr.Zero);

        return handles;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
            hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
        uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [DllImport("user32.dll")]
    internal static extern bool GetWindowCompositionAttribute(IntPtr point, ref WindowCompositionAttributeData data);


    private void WinEventProc(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
        SetBlur();
        if (idObject != 0 || idChild != 0) return;

        var raise = TaskbarPositionChanged;
        if (raise != null) raise(null, null);
    }

    public void PositionChangedHook() {
        procDelegate = WinEventProc;
        hhook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero,
            procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
    }

    public void PositionChangedUnhook() => UnhookWinEvent(hhook);

    public void SetAlpha(double alpha) {
        if (!layered) {
            SetWindowLong(HWND, GWL_EXSTYLE, (uint) GetWindowLong(HWND, GWL_EXSTYLE).ToInt32() | WS_EX_LAYERED);
            layered = true;
        }

        if (_alpha != (byte) (alpha * 255)) {
            SetLayeredWindowAttributes(HWND, 0, (byte) (alpha * 255), LWA_ALPHA);
            _alpha = (byte) (alpha * 255);
        }
    }

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam,
        [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    public void SetBlur() {
        var accent = new AccentPolicy();
        switch (Style) {
            case TaskbarStyle.Default:
                return;
            case TaskbarStyle.ForceDefault:
                Style = TaskbarStyle.Default;

                SendMessage(HWND, WM_DWMCOMPOSITIONCHANGED, 1, null);

                return;
            case TaskbarStyle.Blur:
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                break;
            case TaskbarStyle.Transparent:
                accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
                break;
            default: return;
        }

        unchecked {
            accent.GradientColor = 0x01ffffff;
        }

        ;
        accent.AccentFlags = 2;
        accent.AnimationId = 2;

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData();
        data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
        data.SizeOfData = accentStructSize;
        data.Data = accentPtr;

        GetWindowCompositionAttribute(HWND, ref data);


        SetWindowCompositionAttribute(HWND, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    public Point GetPoint() {
        RECT rc;
        GetWindowRect(HWND, out rc);

        return new Point(rc.Left, rc.Top);
    }

    public Rectangle GetRectangle() {
        RECT rc;
        GetWindowRect(HWND, out rc);

        return new Rectangle(rc.Left, rc.Top, rc.Right, rc.Bottom);
    }

    public void PlaceWindowUnder(Taskbar window) {
        UpdateRadius();
        var rect = GetRectangle();

        SetWindowPos(window.windowHelper.HWND, HWND, rect.X, rect.Y, rect.Width - rect.X, rect.Height - rect.Y,
            SWP.NOACTIVATE | SWP.SHOWWINDOW);
    }

    public void SetWindowZUnder(Taskbar window) => SetWindowPos(window.windowHelper.HWND, HWND, 0, 0, 0, 0,
        SWP.NOREDRAW | SWP.NOACTIVATE | SWP.SHOWWINDOW | SWP.NOREPOSITION | SWP.NOMOVE | SWP.NOSIZE);

    //SetWindowLong(HWND, GWL_EXSTYLE, (uint)((int)GetWindowLong(HWND, GWL_EXSTYLE) | WS_EX_LAYERED));
    [DllImport("shell32.dll")]
    private static extern IntPtr SHAppBarMessage(uint dwMessage,
        ref APPBARDATA pData);

    private static bool IsAutoHide() {
        var appbar = new APPBARDATA {cbSize = Marshal.SizeOf(typeof(APPBARDATA))};
        var state = (uint) SHAppBarMessage(4, ref appbar);
        return (state & 1) == 1;
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport("gdi32.dll")]
    private static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2,
        CombineRgnStyles fnCombineMode);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

    private static float GetScalingFactor() {
        var desktop = GetDC(IntPtr.Zero);
        var real = GetDeviceCaps(desktop, 10);
        var fals = GetDeviceCaps(desktop, 117);

        var scale = fals / (float) real;
        ReleaseDC(IntPtr.Zero, desktop);
        return scale;
    }

    public IntPtr UpdateRadius() {
        if (old_radius != Radius) {
            old_radius = Radius;

            var scale = GetScalingFactor();

            var r = GetRectangle();

            var w = (int) ((r.Width - r.Left) * scale);
            var h = (int) ((r.Height - r.Top) * scale);

            if (!autoHide && WindowHelper.Count > 1) {
                rgn = CreateRectRgn(0, 0, 0, 0);
                if (last) {
                    var rgn2 = CreateRectRgn(0, 0, w / 2, h + 1);
                    var rgn1 = CreateRoundRectRgn(w / 3, 0, w + 1, h + 1, Radius, Radius);
                    CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                    SetWindowRgn(HWND, rgn, true);

                    DeleteObject(rgn1);
                    DeleteObject(rgn2);
                }
                else if (first){
                    var rgn1 = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                    var rgn2 = CreateRectRgn(w / 2, 0, w + 1, h + 1);
                    CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                    SetWindowRgn(HWND, rgn, true);

                    DeleteObject(rgn1);
                    DeleteObject(rgn2);
                }
            }
            else {
                rgn = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                SetWindowRgn(HWND, rgn, true);
            }

            rgn = CreateRectRgn(0, 0, 0, 0);
            GetWindowRgn(HWND, rgn);
            SetWindowRgn(window.windowHelper.HWND, rgn, true);
        }

        return rgn;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left; // x position of upper-left corner
        public int Top; // y position of upper-left corner
        public int Right; // x position of lower-right corner
        public int Bottom; // y position of lower-right corner
    }

    public static class SWP {
        public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
    }

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    internal enum AccentState {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA {
        public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public int lParam;
    }
}
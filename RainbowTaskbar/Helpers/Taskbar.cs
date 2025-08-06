using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

    private float scale;
    private byte _alpha = 255;

    public bool autoHide = IsAutoHide();

    public bool first = false;
    public bool last = false;

    private IntPtr hhook;

    public IntPtr HWND = (IntPtr) 0;

    private bool layered;
    private int old_radius = 0;

    private WinEventDelegate procDelegate;
    private WinEventDelegate proc2Delegate;
    public int Radius = 0;
    public int YOffset = 0;


    private IntPtr rgn = IntPtr.Zero;
    public bool Secondary;
    public TaskbarStyle Style = TaskbarStyle.Default;
    public Taskbar window = null;

    public TaskbarHelper(IntPtr hWnd, bool secondary = false) {
        HWND = hWnd;
        Secondary = secondary;
        scale = GetScalingFactor();

        SendMessage(HWND, WM_DWMCOMPOSITIONCHANGED, 1, null);
    }

    public event EventHandler TaskbarPositionChanged;

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);


    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
        int uFlags);

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
        IntPtr lParam);


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
        if (hwnd != HWND) return;
        if (idObject != 0 || idChild != 0) return;

        if (window is not null && window.TaskbarClip is not null) 
            window.TaskbarClip.Rect = new(0, 0, window.Width, window.Height);
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
        if (!IsWindow(HWND)) return;
        if (!layered) {
            SetWindowLong(HWND, GWL_EXSTYLE, (uint) GetWindowLong(HWND, GWL_EXSTYLE).ToInt32() | WS_EX_LAYERED);
            layered = true;
        }
        // I have no idea what the reasoning behind this is, but it probably meant something when i first did it so we're keeping it!
        if (_alpha != (byte) (alpha * 255)) {
            SetLayeredWindowAttributes(HWND, 0, (byte) (alpha * 255), LWA_ALPHA);
            _alpha = (byte) (alpha * 255);
        } else {
            SetLayeredWindowAttributes(HWND, 0, 254, LWA_ALPHA);
            _alpha = 254;
        }
        
    }

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam,
        [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);


    public void SetBlur() {
        if (!IsWindow(HWND)) return;
        if (ExplorerTAP.ExplorerTAP.NeedsTAPCache) return;
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
            accent.GradientColor = 0x01000000;
        }

        ;
        accent.AccentFlags = 0;
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

    public System.Drawing.Point GetPoint() {
        if (!IsWindow(HWND)) return new System.Drawing.Point(0,0);
        RECT rc;
        GetWindowRect(HWND, out rc);

        return new System.Drawing.Point(rc.Left, rc.Top);
    }
    private int oldActualHeight = 0;
    public Rectangle GetRealRectangle() {
        if (!IsWindow(HWND)) return new Rectangle(0, 0, 0, 0);
        RECT rc;
        GetWindowRect(HWND, out rc);
        return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left, rc.Bottom - rc.Top);
    }
    public Rectangle GetRectangle(bool scaling = true) {
        if (!IsWindow(HWND)) return new Rectangle(0, 0, 0, 0);
        RECT rc;
        GetWindowRect(HWND, out rc);

        if(ExplorerTAP.ExplorerTAP.NeedsTAPCache) {
            var actualPos = ExplorerTAP.ExplorerTAP.GetYPosition(window);
            ;
            rc.Top += (int)(actualPos * (scaling ? GetScalingFactor() : 1));
            //rc.Bottom += actualPos;
        }

        return new Rectangle(rc.Left, rc.Top + YOffset, rc.Right - rc.Left, rc.Bottom - rc.Top - YOffset);
    }

    enum QUERY_USER_NOTIFICATION_STATE {
        QUNS_NOT_PRESENT = 1,
        QUNS_BUSY = 2,
        QUNS_RUNNING_D3D_FULL_SCREEN = 3,
        QUNS_PRESENTATION_MODE = 4,
        QUNS_ACCEPTS_NOTIFICATIONS = 5,
        QUNS_QUIET_TIME = 6,
        QUNS_APP = 7
    };
    [DllImport("shell32.dll")]
    static extern int SHQueryUserNotificationState(
     out QUERY_USER_NOTIFICATION_STATE pquns);


    public void PlaceWindowUnder(Taskbar window) {
        if (!IsWindow(HWND)) return;

        UpdateRadius();
        var rect = GetRectangle();


        SetWindowPos(window.windowHelper.HWND, HWND, rect.X, rect.Y, 0, 0,
            SWP.NOACTIVATE | SWP.SHOWWINDOW | SWP.NOSIZE);
        if (window.Width != rect.Width * (1 / scale)) window.Width = rect.Width * (1/scale);
        var off = App.Settings.GraphicsRepeat ? 0 : window.UnderlayOffset;
        if (window.Height - off != rect.Height * (1 / scale)) window.Height = rect.Height * (1 / scale) - off;


    }


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

    public static int GetSystemDpi() {
        using (Graphics screen = Graphics.FromHwnd(IntPtr.Zero)) {
            IntPtr hdc = screen.GetHdc();

            int virtualWidth = GetDeviceCaps(hdc, 8);
            int physicalWidth = GetDeviceCaps(hdc,118);
            screen.ReleaseHdc(hdc);

            return (int) (96f * physicalWidth / virtualWidth);
        }
    }

    public float GetScalingFactor() {
        if (DPIUtil.IsSupportingDpiPerMonitor) return (float)DPIUtil.ScaleFactor(window);

        var desktop = GetDC(IntPtr.Zero);
        var real = GetDeviceCaps(desktop, 10);
        var fals = GetDeviceCaps(desktop, 117);

        var scale = fals / (float) real;
        ReleaseDC(IntPtr.Zero, desktop);
        return scale;
    }

    [DllImport("gdi32.dll")]
    public static extern int OffsetRgn(IntPtr hrgn, int nXOffset, int nYOffset);
    private TaskbarStyle old_style = TaskbarStyle.ForceDefault;
    public bool UpdateRadius() {

        if (!IsWindow(HWND)) return false;
        if (old_radius != Radius && old_style != Style) {
            old_radius = Radius;
            old_style = Style;

            var scale = GetScalingFactor();

            var r = GetRealRectangle();

            var w = (int) ((r.Width) * scale);
            var h = (int) ((r.Height) * scale);

            if (!autoHide && App.taskbars.Count > 1 && !App.Settings.SameRadiusOnEach) {
                rgn = CreateRectRgn(0, 0, 0, 0);
                if (last) {
                    var rgn2 = CreateRectRgn(0, 0, w / 2, h + 1);
                    var rgn1 = CreateRoundRectRgn(w / 3, 0, w + 1, h + 1, Radius, Radius);
                    CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                    if(SetWindowRgn(HWND, rgn, true) != 0) DeleteObject(rgn);

                    DeleteObject(rgn1);
                    DeleteObject(rgn2);

                    window.Dispatcher.Invoke(() => {
                        window.TaskbarClip.RadiusX = window.TaskbarClip.RadiusY = Radius / 2;
                        window.TaskbarClip.Rect = new(0, 0, window.Width, window.Height);

                        window.TaskbarClipHide.RadiusX = window.TaskbarClipHide.RadiusY = 0;
                        window.TaskbarClipHide.Rect = new(0, 0, Radius / 2, window.Height);
                    });
                }
                else if (first){
                    var rgn1 = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                    var rgn2 = CreateRectRgn(w / 2, 0, w + 1, h + 1);
                    CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                    if(SetWindowRgn(HWND, rgn, true) != 0) DeleteObject(rgn);

                    DeleteObject(rgn1);
                    DeleteObject(rgn2);

                    window.Dispatcher.Invoke(() => {
                        window.TaskbarClip.RadiusX = window.TaskbarClip.RadiusY = Radius / 2;
                        window.TaskbarClip.Rect = new(0, 0, window.Width, window.Height);

                        window.TaskbarClipHide.RadiusX = window.TaskbarClipHide.RadiusY = 0;
                        window.TaskbarClipHide.Rect = new(window.Width - Radius / 2, 0, Radius / 2, window.Height);
                    });
                }
            }
            else {
                rgn = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                if(SetWindowRgn(HWND, rgn, true)!=0) DeleteObject(rgn);

                window.Dispatcher.Invoke(() => {
                    window.TaskbarClip.RadiusX = window.TaskbarClip.RadiusY = Radius / 2;
                    window.TaskbarClip.Rect = new(0, 0, window.Width, window.Height);

                    window.TaskbarClipHide.RadiusX = window.TaskbarClipHide.RadiusY = 0;
                    window.TaskbarClipHide.Rect = new(0, 0, 0, 0);
                });
            }

            rgn = CreateRectRgn(0, 0, 0, 0);
            GetWindowRgn(HWND, rgn);
            if(Style == TaskbarStyle.Default || Style == TaskbarStyle.Blur) {
                if(SetWindowRgn(window.windowHelper.HWND, rgn, true) != 0) DeleteObject(rgn);
            } else {
                var rgn2 = CreateRectRgn(0, 0, w, h);
                if(SetWindowRgn(window.windowHelper.HWND, rgn2, true) != 0) DeleteObject(rgn2);
                DeleteObject(rgn);
            }

            return true;
        }

        return false;
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
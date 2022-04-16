using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace RainbowTaskbar.Helpers;

public class WindowHelper {
    public enum CombineRgnStyles {
        RGN_AND = 1,
        RGN_OR = 2,
        RGN_XOR = 3,
        RGN_DIFF = 4,
        RGN_COPY = 5,
        RGN_MIN = RGN_AND,
        RGN_MAX = RGN_COPY
    }

    public static int Count;

    private static float scale = GetScalingFactor();

    public bool autoHide = IsAutoHide();
    public IntPtr HWND = (IntPtr) 0;
    public int Radius = 0;
    public TaskbarHelper taskbar;


    public Taskbar window;


    public WindowHelper(Taskbar window, TaskbarHelper taskbarHelper) {
        Count++;
        this.window = window;
        Init(taskbarHelper);
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);


    private void TaskbarPosChanged(object sender, EventArgs args) => taskbar.PlaceWindowUnder(window);

    public static IntPtr GetHWND(Window window) => new WindowInteropHelper(Window.GetWindow(window)).EnsureHandle();

    public void Init(TaskbarHelper taskbarHelper) {
        HWND = GetHWND(window);
        SetWindowLong(HWND, (int) GWL.EXSTYLE,
            (uint) GetWindowLong(HWND, (int) GWL.EXSTYLE).ToInt32() | (uint) WS_EX.TRANSPARENT);
        SetWindowLong(HWND, (int) GWL.STYLE,
            (uint) GetWindowLong(HWND, (int) GWL.STYLE).ToInt32() | (uint) WS.POPUP | (uint) WS.VISIBLE);
        taskbar = taskbarHelper;
        taskbarHelper.TaskbarPositionChanged += TaskbarPosChanged;
    }

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

    private static float GetScalingFactor() {
        var desktop = GetDC(IntPtr.Zero);
        var real = GetDeviceCaps(desktop, 10);
        var fals = GetDeviceCaps(desktop, 117);

        var scale = fals / (float) real;
        ReleaseDC(IntPtr.Zero, desktop);
        return scale;
    }

    private enum GWL {
        EXSTYLE = -20,
        HINSTANCE = -6,
        HWNDPARENT = -8,
        ID = -12,
        STYLE = -16,
        USERDATA = -21,
        WNDPROC = -4,

        DWLP_USER = 0x8,
        DWLP_MSGRESULT = 0x0,
        DWLP_DLGPROC = 0x4
    }

    [Flags]
    private enum WS : uint {
        OVERLAPPED = 0x00000000,
        POPUP = 0x80000000,
        CHILD = 0x40000000,
        MINIMIZE = 0x20000000,
        VISIBLE = 0x10000000,
        DISABLED = 0x08000000,
        CLIPSIBLINGS = 0x04000000,
        CLIPCHILDREN = 0x02000000,
        MAXIMIZE = 0x01000000,
        BORDER = 0x00800000,
        DLGFRAME = 0x00400000,
        VSCROLL = 0x00200000,
        HSCROLL = 0x00100000,
        SYSMENU = 0x00080000,
        THICKFRAME = 0x00040000,
        GROUP = 0x00020000,
        TABSTOP = 0x00010000,

        MINIMIZEBOX = 0x00020000,
        MAXIMIZEBOX = 0x00010000,

        CAPTION = BORDER | DLGFRAME,
        TILED = OVERLAPPED,
        ICONIC = MINIMIZE,
        SIZEBOX = THICKFRAME,
        TILEDWINDOW = OVERLAPPEDWINDOW,

        OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,
        POPUPWINDOW = POPUP | BORDER | SYSMENU,
        CHILDWINDOW = CHILD
    }


    private enum WS_EX : uint {
        DLGMODALFRAME = 0x00000001,
        NOPARENTNOTIFY = 0x00000004,
        TOPMOST = 0x00000008,
        ACCEPTFILES = 0x00000010,
        TRANSPARENT = 0x00000020,

        //#if(WINVER >= 0x0400)

        MDICHILD = 0x00000040,
        TOOLWINDOW = 0x00000080,
        WINDOWEDGE = 0x00000100,
        CLIENTEDGE = 0x00000200,
        CONTEXTHELP = 0x00000400,

        RIGHT = 0x00001000,
        LEFT = 0x00000000,
        RTLREADING = 0x00002000,
        LTRREADING = 0x00000000,
        LEFTSCROLLBAR = 0x00004000,
        RIGHTSCROLLBAR = 0x00000000,

        CONTROLPARENT = 0x00010000,
        STATICEDGE = 0x00020000,
        APPWINDOW = 0x00040000,

        OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,
        PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,

        //#endif /* WINVER >= 0x0400 */

        //#if(WIN32WINNT >= 0x0500)

        LAYERED = 0x00080000,

        //#endif /* WIN32WINNT >= 0x0500 */

        //#if(WINVER >= 0x0500)

        NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
        LAYOUTRTL = 0x00400000, // Right to left mirroring

        //#endif /* WINVER >= 0x0500 */

        //#if(WIN32WINNT >= 0x0500)

        COMPOSITED = 0x02000000,
        NOACTIVATE = 0x08000000

        //#endif /* WIN32WINNT >= 0x0500 */
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


    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA {
        public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public TaskbarHelper.RECT rc;
        public int lParam;
    }
}
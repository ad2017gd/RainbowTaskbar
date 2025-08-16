using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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

    public float scale;

    public bool autoHide = IsAutoHide();
    public IntPtr HWND = (IntPtr) 0;
    public int Radius = 0;
    public TaskbarHelper taskbar;


    public Taskbar window;


    public WindowHelper(Taskbar window, TaskbarHelper taskbarHelper) {
        Count++;
        this.window = window;
        Init(taskbarHelper);
        scale = taskbar.GetScalingFactor();
    }

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);


    // size of a device name string
    private const int CCHDEVICENAME = 32;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MonitorInfoEx {
        public int Size;
        public RECT Monitor;
        public RECT WorkArea;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string DeviceName;

        public void Init() {
            this.Size = 40 + 2 * CCHDEVICENAME;
            this.DeviceName = string.Empty;
        }
    }

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, out MonitorInfoEx lpmi);
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject([In] IntPtr hObject);

    [DllImport("user32.dll")]
    static extern IntPtr WindowFromPoint(System.Drawing.Point p);

    

    public event EventHandler? TaskbarUIChanged;
    public string LastUIData = "";

    DateTime lastZIndex = DateTime.MinValue;



    private void TaskbarPosChanged(object sender, EventArgs args) {

        var rect = taskbar.GetRectangle();
        var changed = Math.Abs(window.Left - rect.Left * (1 / scale)) > 0.001 || Math.Abs(window.Top - rect.Top * (1 / scale)) > 0.001 || Math.Abs(window.Width - rect.Width * (1 / scale)) > 0.001 || Math.Abs(window.Height - rect.Height * (1 / scale)) > 0.001;
        if (changed) {
            taskbar.PlaceWindowUnder(window);
            taskbar.SetBlur();
        }

        if (DateTime.Now - lastZIndex > TimeSpan.FromSeconds(0.05) && !changed) {
            lastZIndex = DateTime.Now;
            taskbar.PlaceWindowUnder(window);
            taskbar.SetBlur();
        }

        if(ExplorerTAP.ExplorerTAP.IsInjected) {
            string UIData = ExplorerTAP.ExplorerTAP.GetUIDataStr(window);
            if(UIData != LastUIData) {
                LastUIData = UIData;
                TaskbarUIChanged?.Invoke(this, null);
            }
        }

       

        if (!App.Settings.GraphicsRepeat && App.hiddenWebViewHost is not null && window.Height != rect.Height * (1 / scale)) {
            App.hiddenWebViewHost.Height = App.taskbars.Max(x=>x.Height);
        }
        if(autoHide && changed) {
            var Taskbar = rect;
            var finalW = Taskbar.Width * (1 / scale);
            var finalH = Taskbar.Width * (1 / scale);
            if(finalW > window.MinWidth) {
                window.MinWidth = Taskbar.Width * (1 / scale);
                window.Width = Taskbar.Width * (1 / scale);
            }
            if (finalH > window.MinHeight) {
                window.MinHeight = Taskbar.Height * (1 / scale);
                window.Height = Taskbar.Height * (1 / scale);
            }

            var scr = Screen.FromPoint(new((int)(rect.X*scale), (int)(rect.Y * scale)));
            int max = (int) Math.Max(0,Math.Min(rect.Height, scr.Bounds.Bottom - rect.Y));


            var rgn1 = CreateRectRgn(0, 0, (int)(rect.Width * scale), (int) (rect.Height * scale));
            var rgn2 = CreateRectRgn(0, (int) (max* scale), (int) (rect.Width * scale), (int) (rect.Height * scale));
            var rgn = CreateRectRgn(0, 0, 0, 0);
            CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_XOR);
            if(SetWindowRgn(HWND, rgn, false) != 0) DeleteObject(rgn);
            DeleteObject(rgn1);
            DeleteObject(rgn2);
            
            window.AutoHideClip.RadiusX = window.AutoHideClip.RadiusY = 0;
            window.AutoHideClip.Rect = new(0, max, 1920, rect.Height);

        }
        if (ddHandle != IntPtr.Zero) UpdateDuplicate();
    }

    public static IntPtr GetHWND(Window window) => new WindowInteropHelper(Window.GetWindow(window)).EnsureHandle();
    public bool alive = true;
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    public void Init(TaskbarHelper taskbarHelper) {
        HWND = GetHWND(window);
        SetWindowLong(HWND, (int) GWL.EXSTYLE,
            (uint) GetWindowLong(HWND, (int) GWL.EXSTYLE).ToInt32() | (uint) WS_EX.TRANSPARENT);
        SetWindowLong(HWND, (int) GWL.STYLE,
            (uint) GetWindowLong(HWND, (int) GWL.STYLE).ToInt32() | (uint) WS.POPUP | (uint) WS.VISIBLE);

        
        taskbar = taskbarHelper;
        taskbarHelper.TaskbarPositionChanged += TaskbarPosChanged;

        Task.Run(() => {
            while (alive) {
                window.Dispatcher.Invoke(() => TaskbarPosChanged(null, null));
                Thread.Sleep(20);
            }
        });
    }

    public static void InitOther(Window wnd) {
        var _HWND = GetHWND(wnd);
        SetWindowLong(_HWND, (int) GWL.EXSTYLE,
            (uint) GetWindowLong(_HWND, (int) GWL.EXSTYLE).ToInt32() | (uint) WS_EX.TRANSPARENT);
        SetWindowLong(_HWND, (int) GWL.STYLE,
            (uint) GetWindowLong(_HWND, (int) GWL.STYLE).ToInt32() | (uint) WS.POPUP | (uint) WS.VISIBLE);
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

   

    [DllImport("dwmapi.dll", SetLastError = true)]
    static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left, Top, Right, Bottom;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DWM_THUMBNAIL_PROPERTIES {
        public DWM_TNP dwFlags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte opacity;
        public int fVisible;
        public int fSourceClientAreaOnly;
    }
    public enum DWM_TNP : uint {
        DWM_TNP_RECTDESTINATION = 0x00000001,
        DWM_TNP_RECTSOURCE = 0x00000002,
        DWM_TNP_OPACITY = 0x00000004,
        DWM_TNP_VISIBLE = 0x00000008,
        DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010,
    }
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);
    [DllImport("dwmapi.dll")]
    static extern int DwmUnregisterThumbnail(IntPtr thumb);

    IntPtr ddHandle = IntPtr.Zero;
    public void Duplicate(nint handle) {
        if (ddHandle != IntPtr.Zero) DwmUnregisterThumbnail(ddHandle);
        var r = DwmRegisterThumbnail(this.HWND, handle, out ddHandle);

        ;
    }
    public void UpdateDuplicate() {
        if (ddHandle == IntPtr.Zero) return;
        DWM_THUMBNAIL_PROPERTIES dskThumbProps = new();
        dskThumbProps.dwFlags = DWM_TNP.DWM_TNP_SOURCECLIENTAREAONLY | DWM_TNP.DWM_TNP_VISIBLE | DWM_TNP.DWM_TNP_RECTSOURCE | DWM_TNP.DWM_TNP_OPACITY | DWM_TNP.DWM_TNP_RECTDESTINATION;
        dskThumbProps.fSourceClientAreaOnly = 1;
        dskThumbProps.fVisible = 1;
        dskThumbProps.opacity = 255;
        
        var rect = taskbar.GetRectangle(true);
        rect.X -= (int) App.taskbars.Min(x => x.Left); ;
        
        dskThumbProps.rcSource = new RECT() { Top = 0, Left = rect.X, Right = (int) rect.Width + rect.X, Bottom = (int) rect.Height };
        dskThumbProps.rcDestination = new RECT() { Top = (int) (rect.Top - window.Top * scale), Left = 0, Right = (int) rect.Width, Bottom = (int) rect.Height };
        DwmUpdateThumbnailProperties(ddHandle, ref dskThumbProps);
        ;
    }
    public void RemoveDuplicate() {
        DwmUnregisterThumbnail(ddHandle);
        ddHandle = IntPtr.Zero;
    }

    public enum GWL {
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
    public enum WS : uint {
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


    public enum WS_EX : uint {
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
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RainbowTaskbar.Helpers
{
    public class TaskbarHelper
    {
        public enum TaskbarStyle
        {
            Default,
            Blur,
            Transparent,
            ForceDefault // temporary value
        }

        public event EventHandler TaskbarPositionChanged;

        public IntPtr HWND = (IntPtr)0;
        public Taskbar window = null;
        public TaskbarStyle Style = TaskbarStyle.Default;
        public int Radius = 0;
        public bool Secondary;

        private bool layered = false;
        private byte _alpha = 255;

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static class SWP
        {
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);


        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowCompositionAttribute(IntPtr point, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
        const uint WINEVENT_OUTOFCONTEXT = 0;

        WinEventDelegate procDelegate;


        void WinEventProc(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            SetBlur();
            if (idObject != 0 || idChild != 0)
            {
                return;
            }

            EventHandler raise = TaskbarPositionChanged;
            if (raise != null) raise(null, null);

        }

        public TaskbarHelper(IntPtr hWnd, bool secondary = false)
        {
            HWND = hWnd;
            Secondary = secondary;

            SendMessage(HWND, WM_DWMCOMPOSITIONCHANGED, 1, null);
            

        }

        IntPtr hhook;
        public void PositionChangedHook()
        {
            procDelegate = new WinEventDelegate(WinEventProc);
            hhook = SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero,
                procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public void PositionChangedUnhook()
        {
            UnhookWinEvent(hhook);
        }

        public void SetAlpha(double alpha)
        {
            if (!layered)
            {
                SetWindowLong(HWND, GWL_EXSTYLE, (uint)GetWindowLong(HWND, GWL_EXSTYLE).ToInt32() | WS_EX_LAYERED);
                layered = true;
            }
            if (_alpha != (byte)(alpha * 255))
            {
               
                SetLayeredWindowAttributes(HWND, 0, (byte)(alpha * 255), LWA_ALPHA);
                _alpha = (byte)(alpha * 255);
            }


        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

        public void SetBlur()
        {
            AccentPolicy accent = new AccentPolicy();
            switch (this.Style)
            {
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
            unchecked { accent.GradientColor = (int)0x01ffffff; };
            accent.AccentFlags = 2;
            accent.AnimationId = 2;

            int accentStructSize = Marshal.SizeOf(accent);

            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            WindowCompositionAttributeData data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            GetWindowCompositionAttribute(HWND, ref data);


            SetWindowCompositionAttribute(HWND, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public System.Drawing.Point GetPoint()
        {
            RECT rc;
            GetWindowRect(HWND, out rc);

            return new System.Drawing.Point(rc.Left, rc.Top);
        }

        public System.Drawing.Rectangle GetRectangle()
        {
            RECT rc;
            GetWindowRect(HWND, out rc);

            return new System.Drawing.Rectangle(rc.Left, rc.Top, rc.Right, rc.Bottom);
        }

        public void PlaceWindowUnder(Taskbar window)
        {
            UpdateRadius();
            Rectangle rect = GetRectangle();

            SetWindowPos(window.windowHelper.HWND, HWND, rect.X, rect.Y, rect.Width - rect.X, rect.Height - rect.Y, SWP.NOACTIVATE | SWP.SHOWWINDOW);

        }

        public void SetWindowZUnder(Taskbar window)
        {
            SetWindowPos(window.windowHelper.HWND, HWND, 0, 0, 0, 0, SWP.NOREDRAW | SWP.NOACTIVATE | SWP.SHOWWINDOW | SWP.NOREPOSITION | SWP.NOMOVE | SWP.NOSIZE);
            //SetWindowLong(HWND, GWL_EXSTYLE, (uint)((int)GetWindowLong(HWND, GWL_EXSTYLE) | WS_EX_LAYERED));
           
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }
        [DllImport("shell32.dll")]
        static extern IntPtr SHAppBarMessage(uint dwMessage,
        ref APPBARDATA pData);

        private static bool IsAutoHide()
        {
            APPBARDATA appbar = new APPBARDATA() { cbSize = Marshal.SizeOf(typeof(APPBARDATA)) };
            uint state = (uint)SHAppBarMessage(4, ref appbar);
            return (state & 1) == 1;

        }

        public bool autoHide = IsAutoHide();

        [DllImport("user32.dll")]
        static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, CombineRgnStyles fnCombineMode);

        public enum CombineRgnStyles : int
        {
            RGN_AND = 1,
            RGN_OR = 2,
            RGN_XOR = 3,
            RGN_DIFF = 4,
            RGN_COPY = 5,
            RGN_MIN = RGN_AND,
            RGN_MAX = RGN_COPY
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("user32.dll")]
        static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        static float GetScalingFactor()
        {
            IntPtr desktop = GetDC(IntPtr.Zero);
            int real = GetDeviceCaps(desktop, 10);
            int fals = GetDeviceCaps(desktop, 117);

            float scale = (float)fals / (float)real;
            ReleaseDC(IntPtr.Zero, desktop);
            return scale;
        }

        static float scale = GetScalingFactor();



        private IntPtr rgn = IntPtr.Zero;
        private int old_radius = -1;
        public IntPtr UpdateRadius()
        {

            if (old_radius != Radius)
            {
                old_radius = Radius;

                float scale = GetScalingFactor();

                Rectangle r = GetRectangle();

                int w = (int)((r.Width - r.Left) * scale);
                int h = (int)((r.Height - r.Top) * scale);

                if (!autoHide && WindowHelper.Count > 1)
                {

                    rgn = CreateRectRgn(0, 0, 0, 0);
                    if (Secondary == true)
                    {
                        IntPtr rgn2 = CreateRectRgn(0, 0, w / 2, h + 1);
                        IntPtr rgn1 = CreateRoundRectRgn(w / 3, 0, w + 1, h + 1, Radius, Radius);
                        CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                        SetWindowRgn(HWND, rgn, true);

                        DeleteObject(rgn1);
                        DeleteObject(rgn2);
                    }
                    else
                    {
                        IntPtr rgn1 = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                        IntPtr rgn2 = CreateRectRgn(w / 2, 0, w + 1, h + 1);
                        CombineRgn(rgn, rgn1, rgn2, CombineRgnStyles.RGN_OR);
                        SetWindowRgn(HWND, rgn, true);

                        DeleteObject(rgn1);
                        DeleteObject(rgn2);
                    }

                }
                else
                {
                    rgn = CreateRoundRectRgn(0, 0, w + 1, h + 1, Radius, Radius);
                    SetWindowRgn(HWND, rgn, true);
                }

                rgn = CreateRectRgn(0, 0, 0, 0);
                GetWindowRgn(HWND, rgn);
                SetWindowRgn(window.windowHelper.HWND, rgn, true);

            }

            return rgn;
        }

    }
}

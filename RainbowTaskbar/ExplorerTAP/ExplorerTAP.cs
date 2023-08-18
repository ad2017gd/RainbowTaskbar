using H.Pipes;
using RainbowTaskbar.Configuration.Instructions;
using RainbowTaskbar.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RainbowTaskbar.ExplorerTAP
{
    [StructLayout(LayoutKind.Sequential)]
    struct GUID {
        public uint a;
        public short b;
        public short c;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] d;
    }
    class ExplorerTAP
    {

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lib);
        [DllImport("kernel32.dll")]
        public static extern void FreeLibrary(IntPtr module);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, string proc);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("Windows.UI.Xaml.dll", CharSet = CharSet.Unicode)]
        public static extern int InitializeXamlDiagnosticsEx(string endPointName, uint pid, string xamlDiagDll, string TAPDllName, GUID clsID, string numiPASA);


        public delegate int SetAppearanceTypeDelegate(uint type);
        public delegate int CloseDelegate();

        public static SetAppearanceTypeDelegate SetAppearanceTypeDLL;
        public static CloseDelegate CloseDLL;

        public static IntPtr library;
        public static int tries = 0;

        public static bool IsInjected { get; set; } = false;

        public static void TryInject() {

            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);

            if (
                taskbarHWND != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null) != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "WorkerW", null) == IntPtr.Zero
                ) {
                // We re on win11 with new taskbar

                string dllPath = Environment.GetEnvironmentVariable("temp") + "\\RainbowTaskbarDLL.dll";

                if(File.Exists(dllPath)) {
                    library = LoadLibrary(dllPath);
                    CloseDLL = Marshal.GetDelegateForFunctionPointer<CloseDelegate>(GetProcAddress(library, "CloseDLL"));
                    uint hres = unchecked((uint)CloseDLL());
                    if (hres != 0x800401E3) Thread.Sleep(200);
                }

                try {
                    File.WriteAllBytes(dllPath, Properties.Resources.RainbowTaskbarDLL);
                } catch { }

                library = LoadLibrary(dllPath);

                SetAppearanceTypeDLL = Marshal.GetDelegateForFunctionPointer<SetAppearanceTypeDelegate>(GetProcAddress(library, "SetAppearanceTypeDLL"));
                CloseDLL = Marshal.GetDelegateForFunctionPointer<CloseDelegate>(GetProcAddress(library, "CloseDLL"));


                var guid = new GUID() {
                    a = 0xc9d60190,
                    b = 0x2c89,
                    c = 0x11ee,
                    d = new byte[] { 0xbe, 0x56, 0x02, 0x42, 0xac, 0x12, 0x00, 0x02 }
                };

                uint pid = 0;
                GetWindowThreadProcessId(taskbarHWND, out pid);


                
                var xamlthread = new Thread(() => {
                    InitializeXamlDiagnosticsEx("VisualDiagConnection1", pid, null, dllPath, guid, null);
                });
                xamlthread.Start();
                xamlthread.Join();

                // too lazy to make an event, this shall work
                Task.Delay(1250).Wait();

                
                IsInjected = true;
                tries = 0;
            }

        }
        public static void Reset() {
            if (!IsInjected) return;
            CloseDLL();
            if (library != IntPtr.Zero) FreeLibrary(library);
            IsInjected = false;
        }

        public static void SetAppearanceType(TransparencyInstruction.TransparencyInstructionStyle type) {
            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);
            if (!IsInjected || taskbarHWND == IntPtr.Zero) return;
            int hres = SetAppearanceTypeDLL((uint) type);
            if (unchecked((uint)hres) != 0 && tries++ < 5) { // MK_E_UNAVAILABLE or other errors?
                TryInject();
                if (tries >= 5) {
                    MessageBox.Show(
                        $"0x{hres.ToString("X8")} : {Marshal.GetExceptionForHR(hres)?.Message}\n\nThere seems to be an issue with the RainbowTaskbar DLL injected into explorer.exe. This process is very experimental, so please open up an issue on GitHub (Right-click RainbowTaskbar on system tray -> Submit an issue or request) to try and debug the problem. Make sure to also include any other errors you might have encountered.", "RainbowTaskbar Error", MessageBoxButton.OK, MessageBoxImage.Warning
                        );
                }
            }
        }
    }

}

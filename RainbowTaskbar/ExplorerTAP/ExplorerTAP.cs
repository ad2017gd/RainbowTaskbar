using H.Pipes;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Configuration.InstructionConfig;
using RainbowTaskbar.Configuration.InstructionConfig.Instructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace RainbowTaskbar.ExplorerTAP {
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] uint Msg, IntPtr wParam, IntPtr lParam);


        public delegate int SetAppearanceTypeDelegate(uint type);
        public delegate int CloseDelegate();
        public delegate int VersionDelegate();

        public static SetAppearanceTypeDelegate SetAppearanceTypeDLL;
        public static CloseDelegate CloseDLL;
        public static VersionDelegate VersionDLL;

        public static IntPtr library;
        public static int tries = 0;

        public static bool IsInjected { get; set; } = false;
        public static bool IsInjecting { get; set; } = false;

        public static bool NeedsTAP() {
            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);
            return
                taskbarHWND != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null) != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "WorkerW", null) == IntPtr.Zero;
                
        }

        public const int TAPVERSION = 1;

       
        private static void StartExplorer() {
            string explorer = string.Format("{0}\\{1}", Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
            Process process = new Process();
            process.StartInfo.FileName = explorer;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            do {
                Thread.Sleep(100);
            } while (!NeedsTAP());
        }

        public static bool TryInject() {

            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);

            if (NeedsTAP() && !IsInjecting) {
                if(Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) {
                    MessageBox.Show(App.localization["msgbox_badarch"], "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsInjecting = true;
                    return true;
                }


                IsInjecting = true;
                var inject = true;
                // We re on win11 with new taskbar

                string dllPath = Environment.GetEnvironmentVariable("temp") + "\\RainbowTaskbarDLL.dll";

                if(File.Exists(dllPath)) {
                    bool deleted = false;
                    try {
                        File.Delete(dllPath);
                        deleted = true;
                    }
                    catch { }

                    if (!deleted) {
                        library = LoadLibrary(dllPath);
                        IntPtr VersionDLLPtr = GetProcAddress(library, "VersionDLL");
                        int version = 0;
                        if (VersionDLLPtr != IntPtr.Zero) {
                            VersionDLL = Marshal.GetDelegateForFunctionPointer<VersionDelegate>(VersionDLLPtr);
                            version = VersionDLL();
                        }
                        FreeLibrary(library);
                        if (VersionDLLPtr == IntPtr.Zero || version < TAPVERSION) {
                            var result = MessageBox.Show(
                                App.localization["msgbox_olddll"], "RainbowTaskbar", MessageBoxButton.YesNoCancel, MessageBoxImage.Information
                            );
                            if (result == MessageBoxResult.Yes) {
                                Process.Start("cmd", $"/c taskkill /f /im explorer.exe && del /q \"{dllPath}\"");
                                
                                do {
                                    Thread.Sleep(500);
                                } while (TaskbarHelper.FindWindow("Shell_TrayWnd", null) != IntPtr.Zero);
                                StartExplorer();
                                Thread.Sleep(4000);
                                taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);
                            } else {
                                IsInjecting = true;
                                return true;
                            }
                        } else {
                            inject = false;
                        }
                    }
                    
                    
                }
                try {
                    File.WriteAllBytes(dllPath, Environment.Is64BitOperatingSystem ? Properties.Resources.RainbowTaskbarDLL_x64 : Properties.Resources.RainbowTaskbarDLL_Win32);
                }
                catch { }

                library = LoadLibrary(dllPath);

                if(GetProcAddress(library, "SetAppearanceTypeDLL") != IntPtr.Zero) 
                    SetAppearanceTypeDLL = Marshal.GetDelegateForFunctionPointer<SetAppearanceTypeDelegate>(GetProcAddress(library, "SetAppearanceTypeDLL"));
                if(GetProcAddress(library, "CloseDLL") != IntPtr.Zero) 
                    CloseDLL = Marshal.GetDelegateForFunctionPointer<CloseDelegate>(GetProcAddress(library, "CloseDLL"));

                if(inject) {
                    var guid = new GUID() {
                        a = 0xc9d60190,
                        b = 0x2c89,
                        c = 0x11ee,
                        d = new byte[] { 0xbe, 0x56, 0x02, 0x42, 0xac, 0x12, 0x00, 0x02 }
                    };

                    uint pid = 0;
                    GetWindowThreadProcessId(TaskbarHelper.FindWindow("Shell_TrayWnd", null), out pid);


                    int hr = -1;
                    int tries = 0;
                    do {
                        var xamlthread = new Thread(() => {
                            hr = InitializeXamlDiagnosticsEx("VisualDiagConnection1", pid, null, dllPath, guid, null);
                        });
                        xamlthread.Start();
                        xamlthread.Join();
                        Thread.Sleep(250);
                    } while (hr != 0 && tries++ < 5);

                    // too lazy to make an event, this shall work
                    Task.Delay(1250).Wait();
                }
                
                IsInjecting = false;
                IsInjected = true;
            } else {
                return false;
            }
            return true;

        }
        public static void Reset() {
            if (!IsInjected) return;
            if (CloseDLL is not null)
                CloseDLL();
            if (library != IntPtr.Zero) FreeLibrary(library);
            SetAppearanceTypeDLL = null;
            CloseDLL = null;
            IsInjected = false;
        }

        public static void SetAppearanceType(TransparencyInstruction.TransparencyInstructionStyle type) {
            if (!NeedsTAP() || IsInjecting) return;
            if (SetAppearanceTypeDLL is null) return;
            uint hres = unchecked((uint) SetAppearanceTypeDLL((uint) type));
            if (hres == 0) tries = 0;
            if (hres != 0 && tries < 10) { // MK_E_UNAVAILABLE or other errors?
                while(hres != 0 && tries < 10) {
                    if(TryInject()) 
                        tries++;
                    hres = unchecked((uint) SetAppearanceTypeDLL((uint) type));
                }
                if (hres != 0 && tries >= 10) {
                    MessageBox.Show(
                        $"0x{hres.ToString("X8")} : {Marshal.GetExceptionForHR(unchecked((int) hres))?.Message}\n\n{App.localization["msgbox_baddll"]}", "RainbowTaskbar Error", MessageBoxButton.OK, MessageBoxImage.Warning
                        );
                }


            }
        }
    }

}

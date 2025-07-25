﻿using H.Pipes;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Configuration.Instruction;
using RainbowTaskbar.Configuration.Instruction.Instructions;
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
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;

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
        public delegate int GetDataPtrDelegate();
        public delegate int DebugGetUITreeDelegate(ref IntPtr tree);
        public delegate int GetYPositionDelegate(IntPtr hwnd);

        public static SetAppearanceTypeDelegate SetAppearanceTypeDLL;
        public static CloseDelegate CloseDLL;
        public static VersionDelegate VersionDLL;
        public static GetDataPtrDelegate GetDataPtrDLL;
        public static DebugGetUITreeDelegate DebugGetUITreeDLL;
        public static GetYPositionDelegate GetYPositionDLL;

        public static IntPtr library;
        public static int tries = 0;

        public static bool IsInjected { get; set; } = false;
        public static bool IsInjecting { get; set; } = false;

        public static bool NeedsTAPCache { get; set; } = false;

        public static bool NeedsTAP() {
            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);
            return
                (NeedsTAPCache = taskbarHWND != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null) != IntPtr.Zero &&
                TaskbarHelper.FindWindowEx(taskbarHWND, IntPtr.Zero, "WorkerW", null) == IntPtr.Zero);
                
        }

        public const int TAPVERSION = 2;

       
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
        public static IntPtr dataPtr = IntPtr.Zero;
        static IntPtr proc = IntPtr.Zero;
        static IntPtr str = IntPtr.Zero;

        public static bool TryInject() {

            var taskbarHWND = TaskbarHelper.FindWindow("Shell_TrayWnd", null);

            if (NeedsTAP() && !IsInjecting) {
                if(Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess) {
                    MessageBox.Show(App.localization["msgbox_badarch"], "RainbowTaskbar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    IsInjecting = true;
                    return true;
                }
                if (library != IntPtr.Zero) FreeLibrary(library);
                

                IsInjecting = true;
                var inject = true;
                // We re on win11 with new taskbar

                string dllPath = Environment.GetEnvironmentVariable("temp") + "\\RainbowTaskbarDLL.dll";
                dataPtr = IntPtr.Zero;

                if (File.Exists(dllPath)) {
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
                                Process.Start("cmd", $"/c taskkill /f /im explorer.exe").WaitForExit();
                                Thread.Sleep(100);
                                Process.Start("cmd", $"/c del /q \"{dllPath}\"").WaitForExit();
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
                if (GetProcAddress(library, "GetDataPtrDLL") != IntPtr.Zero)
                    GetDataPtrDLL = Marshal.GetDelegateForFunctionPointer<GetDataPtrDelegate>(GetProcAddress(library, "GetDataPtrDLL"));
                if (GetProcAddress(library, "DebugGetUITreeDLL") != IntPtr.Zero)
                    DebugGetUITreeDLL = Marshal.GetDelegateForFunctionPointer<DebugGetUITreeDelegate>(GetProcAddress(library, "DebugGetUITreeDLL"));
                if (GetProcAddress(library, "GetYPositionDLL") != IntPtr.Zero)
                    GetYPositionDLL = Marshal.GetDelegateForFunctionPointer<GetYPositionDelegate>(GetProcAddress(library, "GetYPositionDLL"));

                if (inject) {
                    var guid = new GUID() {
                        a = 0xc9d60190,
                        b = 0x2c89,
                        c = 0x11ee,
                        d = new byte[] { 0xbe, 0x56, 0x02, 0x42, 0xac, 0x12, 0x00, 0x02 }
                    };

                    uint pid = 0;
                    GetWindowThreadProcessId(TaskbarHelper.FindWindow("Shell_TrayWnd", null), out pid);


                    int hr = -1;
                    int tries = 1;
                    do {
                        var xamlthread = new Thread(() => {
                            hr = InitializeXamlDiagnosticsEx($"VisualDiagConnection{tries}", pid, null, dllPath, guid, null);
                        });
                        xamlthread.Start();
                        xamlthread.Join();
                        Thread.Sleep(50);
                    } while (hr != 0 && tries++ < 50);

                    // too lazy to make an event, this shall work
                    Task.Delay(1250).Wait();
                }

                if (proc != IntPtr.Zero) CloseHandle(proc);
                if (str != IntPtr.Zero) Marshal.FreeHGlobal(str);

                GetWindowThreadProcessId(taskbarHWND, out var pidd);
                proc = OpenProcess(ProcessAccessFlags.VirtualMemoryRead, false, pidd);
                str = Marshal.AllocHGlobal(Marshal.SizeOf<RainbowTaskbarData>());

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
        public static int GetDataPtr() {
            if (!NeedsTAP() || IsInjecting) return -1;
            if (GetDataPtrDLL is null) return -1;
            return GetDataPtrDLL();
        }


        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct TaskbarInfo {
            public IntPtr taskbar;
            public int YPosition;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[,] _reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct RainbowTaskbarData {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public TaskbarInfo[] lTaskbarInfo;

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             ProcessAccessFlags processAccess,
             bool bInheritHandle,
             uint processId
        );
        [Flags]
        public enum ProcessAccessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);




        public static int GetYPosition(Taskbar t) {
            if (!NeedsTAP() || IsInjecting) return 0;

            if (dataPtr == IntPtr.Zero) dataPtr = GetDataPtr();
            ReadProcessMemory(proc, dataPtr, str, Marshal.SizeOf<RainbowTaskbarData>(), out _);
            var data = Marshal.PtrToStructure<RainbowTaskbarData>(str);

            var x = data.lTaskbarInfo.FirstOrDefault(x => x.taskbar == t.taskbarHelper.HWND, new()).YPosition;
            return x;
        }
        public static string DebugGetUITree() {
            if (!NeedsTAP() || IsInjecting) return null;
            if (DebugGetUITreeDLL is null) return null;
            IntPtr bstr = IntPtr.Zero;
            var a = DebugGetUITreeDLL(ref bstr);
            string str = Marshal.PtrToStringBSTR(bstr);
            Marshal.FreeBSTR(bstr);

            return str;
        }
    }

}

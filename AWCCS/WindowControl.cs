using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Runtime.InteropServices;

/*
 Notes
 * http://www.blitzbasic.com/Community/posts.php?topic=28574
 * http://stackoverflow.com/questions/11214409/determine-if-window-has-close-button-or-why-isnt-gettitlebarinfo-working
 * http://stackoverflow.com/questions/7277366/why-does-enumwindows-return-more-windows-than-i-expected
 * http://stackoverflow.com/questions/1888863/how-to-get-main-window-handle-from-process-id
 * 
 * https://msdn.microsoft.com/en-us/library/vstudio/2ab8kd75%28v=vs.100%29.aspx
 * 
 * http://stackoverflow.com/questions/277085/how-do-i-getmodulefilename-if-i-only-have-a-window-handle-hwnd
 */

namespace WindowsFormsApplication1
{
    class WindowControl
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        //
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetTitleBarInfo(IntPtr hwnd, ref TITLEBARINFO pti);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        //http://lolionnet.blogspot.com.au/2015/03/c-api-getprocessimagefilename.html
        [DllImport("psapi.dll")]
        static extern uint GetProcessImageFileName([In] IntPtr hProcess,[Out][MarshalAs(UnmanagedType.LPStr)] StringBuilder lpImageFileName,[In] uint nSize);
        
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
 
        //
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_TOPMOST = 0x00000008;


        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFO
        {
            public int cbSize;
            //public RECT rcTitleBar; //Dnt need
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size++ > 0)
            {

                //
                TITLEBARINFO titleBarInfo = new TITLEBARINFO();
                titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                GetTitleBarInfo(hWnd, ref titleBarInfo);

                if (!IsWindowVisible(hWnd)) 
                {
                    return String.Empty;
                }

                //check for correct flags
                if ((titleBarInfo.rgstate[0] & 0x00008000) == 0x00008000)//STATE_SYSTEM_INVISIBLE)
                {
                    return String.Empty;
                }

                if ((GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW )>0) 
                {
                    return String.Empty;
                }

                var builder = new StringBuilder(size);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        public static int CheckIfWindowAlreadyOnTop(IntPtr hWnd) 
        {

            if (((GetWindowLong(hWnd, GWL_EXSTYLE)) & WS_EX_TOPMOST) != 0)
            {
                // window is top most
                return 1;

            }

            //Window is not top most
            return 0;
        }

        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (GetWindowText(wnd).Contains(titleText))
                {
                    windows.Add(wnd);
                }
                return true;
            },
                    IntPtr.Zero);

            return windows;
        } // closing bracket

        public static System.String getProcessFileName(uint processID)
        {
            const int nChars = 1024;

            StringBuilder filename = new StringBuilder(nChars);
            IntPtr hProcess = OpenProcess(1040, 0, processID);
            GetModuleFileNameEx(hProcess,IntPtr.Zero,filename,nChars);
            CloseHandle(hProcess);

            return filename.ToString();
        }

        //
        public static System.Collections.ArrayList getAllWindowNames() 
        {
            System.Collections.ArrayList windowNames = new System.Collections.ArrayList();

            IntPtr found = IntPtr.Zero;


            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                uint processId = 0;

                WindowData currentData = new WindowData();

                currentData.WindowText = GetWindowText(wnd);
                currentData.windowID = wnd.ToString();
                currentData.imageData = WindowIcon.GetSmallWindowIcon(wnd);
                
                //process ID
                GetWindowThreadProcessId(wnd,out processId);
                currentData.processID = processId.ToString();

                currentData.imageName = getProcessFileName(processId);

                if (currentData.WindowText.Length != 0)
                {
                    currentData.isTopMost = CheckIfWindowAlreadyOnTop(wnd);

                    windowNames.Add(currentData);
                }

                return true;
            },
                    IntPtr.Zero);

            return windowNames;
        }

        public static void setWindowState(String processName, bool alwaysOnTop) 
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                String windowText = GetWindowText(wnd);

                if (processName == wnd.ToString())
                {

                    /*
                     * https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx
                     */

                    //second param as -1 will make always ontop
                    if (alwaysOnTop == false)
                    {
                        //normal
                        SetWindowPos(wnd, -2, 1, 1, 1, 1, 0x01 + 0x02 + 0x10 + 0x0200);
                    }
                    else 
                    {
                        //top most
                        SetWindowPos(wnd, -1, 1, 1, 1, 1, 0x01 + 0x02 + 0x10 + 0x0200);
                    }
                }

                return true;
            },
                    IntPtr.Zero);
        }

        public static IEnumerable<IntPtr> FindAllWindowst() 
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                String windowText = GetWindowText(wnd);

                if (windowText.Length != 0)
                {
                    System.Diagnostics.Debug.Write(windowText+"\n");

                    windows.Add(wnd);

                    //second param as -1 will make always ontop

                    SetWindowPos(wnd,1,1,1,1,1,1);
                }

                return true;
            },
                    IntPtr.Zero);

            return windows;
        }
    }
}

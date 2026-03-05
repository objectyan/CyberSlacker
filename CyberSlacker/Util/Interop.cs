using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CyberSlacker.Util
{
    public static class Interop
    {
        [DllImport("user32.dll")]
        public static extern uint GetDpiForWindow(nint hwnd);
        [DllImport("user32.dll")]
        public static extern nint GetParent(nint hWnd);
        [DllImport("user32.dll")]
        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(nint hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetFocus(nint hWnd);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(nint hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(nint hWnd);
        [DllImport("user32.dll")]
        public static extern nint WindowFromPoint(POINT Point);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(nint hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(nint hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);
        public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        [DllImport("user32.dll")]
        public static extern nint SetParent(nint hWndChild, nint hWndNewParent);
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int ExtractIconEx(string lpszFile, int nIconIndex, nint[] phiconLarge, nint[]? phiconSmall, int nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(nint hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public struct WINDOWPOS
        {
            public nint hwnd;
            public nint hwndInsertAfter;
            public uint x;
            public uint y;
            public uint cx;
            public uint cy;
            public uint flags;
        }

        public const uint GW_HWNDPREV = 3;
        public const uint GW_HWNDNEXT = 2;
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_CHILD = 0x40000000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const int SW_SHOWNA = 8;

        [DllImport("user32.dll")]
        public static extern nint GetTopWindow(nint hWnd);

        [DllImport("user32.dll")]
        public static extern nint GetWindow(nint hWnd, uint uCmd);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern nint GetWindowLong32(nint hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern nint GetWindowLong64(nint hWnd, int nIndex);

        public static nint GetWindowLong(nint hWnd, int nIndex)
        {
            return nint.Size == 8 ? GetWindowLong64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }


        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

        public static nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong)
        {
            return nint.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new nint(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList lParam);

        public delegate bool EnumedWindow(nint hwnd, ArrayList lParam);

        public static bool EnumWindowCallback(nint hwnd, ArrayList lParam)
        {
            lParam.Add(hwnd);
            return true;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint FindWindowEx(nint hWndParent, nint hWndChildAfter, string lpszClass, string lpszWindow);
        public const int WM_NCACTIVATE = 0x0086;
        public const uint WM_SETREDRAW = 0x000B;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_MOVING = 0x0216;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KILLFOCUS = 0x0008;
        public const int WM_SIZE = 0x0005;
        public const int SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOZORDER = 0x0004;


        public const uint SHGFI_ICON = 0x000000100;      // Get icon
        public const uint SHGFI_LARGEICON = 0x000000000; // Large icon (default)
        public const uint SHGFI_SMALLICON = 0x000000001; // Small icon


        [DllImport("user32.dll")] public static extern bool ReleaseCapture();


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int width, int height, uint uFlags);
        public const uint SWP_NOACTIVATE = 0x0010;

        public static readonly nint HWND_TOPMOST = new nint(-1);
        public static readonly nint HWND_NOTOPMOST = new nint(-2);
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
       
        public const uint SEE_MASK_INVOKEIDLIST = 0x0000000C;


    }

}

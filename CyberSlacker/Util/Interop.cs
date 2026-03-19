using System.Runtime.InteropServices;

namespace CyberSlacker.Util
{
    /// <summary>
    /// Win32 API 互操作类
    /// </summary>
    internal static partial class Interop
    {
        private const string User32 = "user32.dll";
        private const string Shell32 = "shell32.dll";
        private const string Kernel32 = "kernel32.dll";

        #region 窗口与显示 (Window & Display)

        /// <summary> 获取指定窗口的 DPI 缩放值（用于适配高分屏） </summary>
        [LibraryImport(User32)]
        internal static partial uint GetDpiForWindow(nint hwnd);

        /// <summary> 获取指定窗口的父窗口句柄 </summary>
        [LibraryImport(User32)]
        internal static partial nint GetParent(nint hWnd);

        /// <summary> 向窗口发送 Win32 消息（用于模拟拖动、设置状态等） </summary>
        [LibraryImport(User32, EntryPoint = "SendMessageW")]
        internal static partial nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        /// <summary> 标记窗口区域为失效，强制系统进行重绘 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool InvalidateRect(nint hWnd, nint lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

        /// <summary> 立即更新窗口，绕过消息队列直接重绘 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool UpdateWindow(nint hWnd);

        /// <summary> 为指定窗口设置键盘输入焦点 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetFocus(nint hWnd);

        /// <summary> 设置窗口的显示状态（显示、隐藏、最大化等） </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(nint hWnd, int nCmdShow);

        /// <summary> 将指定窗口带到前台并激活 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(nint hWnd);

        /// <summary> 获取屏幕特定坐标点下的窗口句柄 </summary>
        [LibraryImport(User32)]
        internal static partial nint WindowFromPoint(POINT Point);

        /// <summary> 根据类名或窗口标题查找顶级窗口 </summary>
        [LibraryImport(User32, EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        internal static partial nint FindWindow(string lpClassName, string lpWindowName);

        /// <summary> 将屏幕坐标转换为特定窗口的客户端相对坐标 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ScreenToClient(nint hWnd, ref POINT lpPoint);

        /// <summary> 检查句柄是否指向一个现有的有效窗口 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool IsWindow(nint hWnd);

        /// <summary> 注册一个全局唯一的 Windows 消息 ID </summary>
        [LibraryImport(User32, EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial uint RegisterWindowMessage(string lpString);

        /// <summary> 遍历屏幕上所有的顶级窗口 </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);
        internal delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        /// <summary> 改变指定窗口的父级（用于将挂件钉在桌面背景层） </summary>
        [LibraryImport(User32)]
        internal static partial nint SetParent(nint hWndChild, nint hWndNewParent);

        /// <summary> 检查特定按键的实时按下状态（用于实现老板键） </summary>
        [LibraryImport(User32)]
        internal static partial short GetAsyncKeyState(int vKey);

        /// <summary> 从文件（EXE/DLL）中提取大图标或小图标 </summary>
        [LibraryImport(Shell32, EntryPoint = "ExtractIconExW", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial int ExtractIconEx(string lpszFile, int nIconIndex, [Out] nint[] phiconLarge, [Out] nint[]? phiconSmall, int nIcons);

        /// <summary> 获取窗口在屏幕上的绝对位置矩形 </summary>
        [LibraryImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetWindowRect(nint hwnd, out RECT lpRect);

        /// <summary> 获取窗口层级树中顶层的窗口 </summary>
        [LibraryImport(User32)]
        internal static partial nint GetTopWindow(nint hWnd);

        /// <summary> 获取与指定窗口有特定关系（上下层、前后）的窗口句柄 </summary>
        [LibraryImport(User32)]
        internal static partial nint GetWindow(nint hWnd, uint uCmd);

        #endregion

        #region 窗口样式操作 (Get/Set WindowLong)

        // 针对 32 位和 64 位系统的兼容性处理
        [LibraryImport(User32, EntryPoint = "GetWindowLongW")]
        private static partial nint GetWindowLong32(nint hWnd, int nIndex);

        [LibraryImport(User32, EntryPoint = "GetWindowLongPtrW")]
        private static partial nint GetWindowLong64(nint hWnd, int nIndex);

        /// <summary> 获取窗口的样式信息 </summary>
        internal static nint GetWindowLong(nint hWnd, int nIndex)
        {
            return nint.Size == 8 ? GetWindowLong64(hWnd, nIndex) : GetWindowLong32(hWnd, nIndex);
        }

        [LibraryImport(User32, EntryPoint = "SetWindowLongW")]
        private static partial int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

        [LibraryImport(User32, EntryPoint = "SetWindowLongPtrW")]
        private static partial nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

        /// <summary> 更改窗口的样式属性（如移除标题栏、设置点击穿透等） </summary>
        internal static nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong)
        {
            return nint.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new nint(SetWindowLong32(hWnd, nIndex, (int)dwNewLong));
        }

        #endregion

        #region 布局与硬件 (Layout & Hardware)

        /// <summary> 释放鼠标捕获（配合 SendMessage 用于无标题栏拖动窗口） </summary>
        [LibraryImport(User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ReleaseCapture();

        /// <summary> 改变窗口的位置、大小及 Z 序 </summary>
        [LibraryImport(User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int width, int height, uint uFlags);

        /// <summary> 获取当前系统的物理和虚拟内存状态 </summary>
        [LibraryImport(Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        /// <summary> 查找特定窗口下的子窗口（用于定位桌面图标层） </summary>
        [LibraryImport(User32, EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        internal static partial nint FindWindowEx(nint hWndParent, nint hWndChildAfter, string lpszClass, string lpszWindow);

        #endregion

        #region 常量定义 (Constants)

        internal const int GWL_EXSTYLE = -20;           // 扩展窗口样式索引
        internal const int GWL_STYLE = -16;             // 基础窗口样式索引
        internal const int WS_EX_NOACTIVATE = 0x08000000; // 窗口不通过点击激活
        internal const int WS_EX_TOOLWINDOW = 0x00000080; // 工具窗口，不显示在任务栏
        internal const int WS_CHILD = 0x40000000;         // 子窗口样式
        internal const int WS_POPUP = unchecked((int)0x80000000); // 弹出窗口样式

        internal const uint SWP_NOSIZE = 0x0001;        // 移动时不改变大小
        internal const uint SWP_NOMOVE = 0x0002;        // 改变大小时不移动位置
        internal const uint SWP_NOZORDER = 0x0004;      // 不改变 Z 序
        internal const uint SWP_SHOWWINDOW = 0x0040;    // 移动/缩放后显示窗口
        internal const uint SWP_NOACTIVATE = 0x0010;    // 不激活窗口
        internal const uint SWP_NOREDRAW = 0x0008;      // 禁止自动重绘

        internal const int WM_NCLBUTTONDOWN = 0x00A1;   // 非客户区左键按下
        internal const int HTCAPTION = 2;               // 标题栏命中测试代码
        internal const int WM_MOUSEACTIVATE = 0x0021;   // 鼠标点击激活消息

        internal static readonly nint HWND_TOPMOST = new(-1);    // 窗口始终置顶
        internal static readonly nint HWND_NOTOPMOST = new(-2); // 取消窗口置顶

        #endregion

        #region 结构体定义 (Structs)

        /// <summary> 屏幕坐标点结构 </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT { public int X; public int Y; }

        /// <summary> 矩形区域结构 </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary> 内存状态详细结构 </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MEMORYSTATUSEX
        {
            public uint dwLength;             // 结构体大小
            public uint dwMemoryLoad;         // 内存使用率 (%)
            public ulong ullTotalPhys;        // 总物理内存
            public ulong ullAvailPhys;        // 可用物理内存
            public ulong ullTotalPageFile;    // 总页文件大小
            public ulong ullAvailPageFile;    // 可用页文件大小
            public ulong ullTotalVirtual;     // 总虚拟内存
            public ulong ullAvailVirtual;     // 可用虚拟内存
            public ulong ullAvailExtendedVirtual; // 预留
        }

        #endregion
    }
}
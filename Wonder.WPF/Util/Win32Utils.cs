using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Wonder.WPF.Util
{
    internal static class Win32Utils
    {
        public const int MONITOR_DEFAULTTONULL = 0x00000000;
        public const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        public static ushort LOWORD(uint value)
            => (ushort)(value & 0xFFFF);

        public static ushort HIWORD(uint value)
            => (ushort)(value >> 16);

        public static byte LOBYTE(uint value)
            => (byte)(value & 0xFF);

        public static byte HIBYTE(uint value)
            => (byte)(value >> 8);

        #region P/Invoke

        /// <summary>
        /// Win32: 获取显示器信息
        /// </summary>
        /// <see cref="https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-getmonitorinfoa"/>
        /// <param name="hMonitor">显示器句柄</param>
        /// <param name="lpmi">指向接收指定显示器信息的 <see cref="MONITORINFO"/> 的指针</param>
        /// <returns>获取操作是否成功</returns>
        [SecurityCritical]
        [DllImport("user32.dll", EntryPoint = "GetMonitorInfo", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// Win32: 获取与指定窗口矩形区域面积交集有最大值的显示器句柄
        /// </summary>
        /// <see cref="https://docs.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-monitorfromwindow"/>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="dwFlags">决定如果窗口与任意显示器都没有交集时的返回值</param>
        /// <returns>显示器句柄，若未找到返回 <see cref="IntPtr.Zero"/></returns>
        [SecurityCritical]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        /// <summary>
        /// Win32: 改变指定窗口的位置和尺寸
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-movewindow"/>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="x">水平位置</param>
        /// <param name="y">垂直位置</param>
        /// <param name="nWidth">窗口宽度</param>
        /// <param name="nHeight">窗口高度</param>
        /// <param name="bRepaint">重绘标识</param>
        /// <returns>移动操作是否成功</returns>
        [SecurityCritical]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);
        #endregion
    }

    public struct POINT
    {
        public POINT(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            var value = obj is POINT another
                      ? another.X == X && another.Y == Y
                      : false;
            return value;
        }

        public override int GetHashCode()
        {
            var code = HashCode.Combine(X, Y);
            return code;
        }

        public static bool operator ==(POINT left, POINT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(POINT left, POINT right)
        {
            return !(left == right);
        }
    }

    public struct SIZE
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public SIZE(int width, int height)
            : this()
        {
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            var value = obj is SIZE another
                      ? another.Width == Width && another.Height == Height
                      : false;
            return value;
        }

        public override int GetHashCode()
        {
            var code = HashCode.Combine(Width, Height);
            return code;
        }

        public static bool operator ==(SIZE left, SIZE right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SIZE left, SIZE right)
        {
            return !(left == right);
        }
    }

    public struct RECT
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public POINT Position => new POINT(Left, Top);
        public SIZE Size => new SIZE(Width, Height);

        public RECT(int left, int top, int right, int bottom)
            : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public void Offset(int dx, int dy)
        {
            Left += dx;
            Top += dy;
            Right += dx;
            Bottom += dy;
        }

        public override bool Equals(object obj)
        {
            if (obj is RECT another)
            {
                var value = Left == another.Left &&
                            Top == another.Top &&
                            Right == another.Right &&
                            Bottom == another.Bottom;
                return value;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var code = HashCode.Combine(Left, Top, Right, Bottom);
            return code;
        }

        public static RECT Union(RECT rect1, RECT rect2)
        {
            var left = Math.Min(rect1.Left, rect2.Left);
            var top = Math.Min(rect1.Top, rect2.Top);
            var right = Math.Max(rect1.Right, rect2.Right);
            var bottom = Math.Max(rect1.Bottom, rect2.Bottom);
            var rect = new RECT(left, top, right, bottom);
            return rect;
        }

        public static bool operator ==(RECT left, RECT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RECT left, RECT right)
        {
            return !(left == right);
        }
    }

    public struct MINMAXINFO
    {
        public POINT Reserved { get; set; }
        public POINT MaxSize { get; set; }
        public POINT MaxPosition { get; set; }
        public POINT MinTrackSize { get; set; }
        public POINT MaxTrackSize { get; set; }

        public override bool Equals(object obj)
        {
            var value = obj is MINMAXINFO another
                      ? another.Reserved == Reserved &&
                        another.MaxSize == MaxSize &&
                        another.MaxPosition == MaxPosition &&
                        another.MinTrackSize == MinTrackSize &&
                        another.MaxTrackSize == MaxTrackSize
                      : false;
            return value;
        }

        public override int GetHashCode()
        {
            var code = HashCode.Combine(Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize);
            return code;
        }

        public static bool operator ==(MINMAXINFO left, MINMAXINFO right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MINMAXINFO left, MINMAXINFO right)
        {
            return !(left == right);
        }
    }

    public struct MONITORINFO
    {
        public int Size { get; set; }
        public RECT MonitorRect { get; set; }
        public RECT WorkRect { get; set; }
        public int Flags { get; set; }

        public override bool Equals(object obj)
        {
            var value = obj is MONITORINFO another
                      ? another.Size == Size &&
                        another.MonitorRect == MonitorRect &&
                        another.WorkRect == WorkRect &&
                        another.Flags == Flags
                      : false;
            return value;
        }

        public override int GetHashCode()
        {
            var code = HashCode.Combine(Size, MonitorRect, WorkRect, Flags);
            return code;
        }

        public static bool operator ==(MONITORINFO left, MONITORINFO right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MONITORINFO left, MONITORINFO right)
        {
            return !(left == right);
        }
    }

    public enum Win32Codes
    {
        WM_NULL = 0x0000,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000A,
        WM_SETREDRAW = 0x000B,
        WM_SETTEXT = 0x000C,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
        WM_QUERYENDSESSION = 0x0011,
        WM_QUIT = 0x0012,
        WM_QUERYOPEN = 0x0013,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_ENDSESSION = 0x0016,
        WM_SHOWWINDOW = 0x0018,
        WM_CTLCOLOR = 0x0019,
        WM_WININICHANGE = 0x001A,
        WM_SETTINGCHANGE = 0x001A,
        WM_DEVMODECHANGE = 0x001B,
        WM_ACTIVATEAPP = 0x001C,
        WM_FONTCHANGE = 0x001D,
        WM_TIMECHANGE = 0x001E,
        WM_CANCELMODE = 0x001F,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,
        WM_GETMINMAXINFO = 0x0024,
        WM_PAINTICON = 0x0026,
        WM_ICONERASEBKGND = 0x0027,
        WM_NEXTDLGCTL = 0x0028,
        WM_SPOOLERSTATUS = 0x002A,
        WM_DRAWITEM = 0x002B,
        WM_MEASUREITEM = 0x002C,
        WM_DELETEITEM = 0x002D,
        WM_VKEYTOITEM = 0x002E,
        WM_CHARTOITEM = 0x002F,
        WM_SETFONT = 0x0030,
        WM_GETFONT = 0x0031,
        WM_SETHOTKEY = 0x0032,
        WM_GETHOTKEY = 0x0033,
        WM_QUERYDRAGICON = 0x0037,
        WM_COMPAREITEM = 0x0039,
        WM_GETOBJECT = 0x003D,
        WM_COMPACTING = 0x0041,
        WM_COMMNOTIFY = 0x0044,
        WM_WINDOWPOSCHANGING = 0x0046,
        WM_WINDOWPOSCHANGED = 0x0047,
        WM_POWER = 0x0048,
        WM_COPYDATA = 0x004A,
        WM_CANCELJOURNAL = 0x004B,
        WM_NOTIFY = 0x004E,
        WM_INPUTLANGCHANGEREQUEST = 0x0050,
        WM_INPUTLANGCHANGE = 0x0051,
        WM_TCARD = 0x0052,
        WM_HELP = 0x0053,
        WM_USERCHANGED = 0x0054,
        WM_NOTIFYFORMAT = 0x0055,

        WM_CONTEXTMENU = 0x007B,
        WM_STYLECHANGING = 0x007C,
        WM_STYLECHANGED = 0x007D,
        WM_DISPLAYCHANGE = 0x007E,
        WM_GETICON = 0x007F,
        WM_SETICON = 0x0080,
        WM_NCCREATE = 0x0081,
        WM_NCDESTROY = 0x0082,
        WM_NCCALCSIZE = 0x0083,
        WM_NCHITTEST = 0x0084,
        WM_NCPAINT = 0x0085,
        WM_NCACTIVATE = 0x0086,
        WM_GETDLGCODE = 0x0087,
        WM_SYNCPAINT = 0x0088,
        WM_MOUSEQUERY = 0x009B,
        WM_NCMOUSEMOVE = 0x00A0,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONUP = 0x00A2,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_NCRBUTTONUP = 0x00A5,
        WM_NCRBUTTONDBLCLK = 0x00A6,
        WM_NCMBUTTONDOWN = 0x00A7,
        WM_NCMBUTTONUP = 0x00A8,
        WM_NCMBUTTONDBLCLK = 0x00A9,
        WM_NCXBUTTONDOWN = 0x00AB,
        WM_NCXBUTTONUP = 0x00AC,
        WM_NCXBUTTONDBLCLK = 0x00AD,
        WM_INPUT = 0x00FF,
        WM_KEYFIRST = 0x0100,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,

        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,
        WM_KEYLAST = 0x0108,
        WM_IME_STARTCOMPOSITION = 0x010D,
        WM_IME_ENDCOMPOSITION = 0x010E,
        WM_IME_COMPOSITION = 0x010F,
        WM_IME_KEYLAST = 0x010F,
        WM_INITDIALOG = 0x0110,

        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_TIMER = 0x0113,
        WM_HSCROLL = 0x0114,
        WM_VSCROLL = 0x0115,
        WM_INITMENU = 0x0116,
        WM_INITMENUPOPUP = 0x0117,
        WM_MENUSELECT = 0x011F,
        WM_MENUCHAR = 0x0120,
        WM_ENTERIDLE = 0x0121,
        WM_UNINITMENUPOPUP = 0x0125,
        WM_CHANGEUISTATE = 0x0127,
        WM_UPDATEUISTATE = 0x0128,
        WM_QUERYUISTATE = 0x0129,
        WM_CTLCOLORMSGBOX = 0x0132,
        WM_CTLCOLOREDIT = 0x0133,
        WM_CTLCOLORLISTBOX = 0x0134,
        WM_CTLCOLORBTN = 0x0135,
        WM_CTLCOLORDLG = 0x0136,
        WM_CTLCOLORSCROLLBAR = 0x0137,
        WM_CTLCOLORSTATIC = 0x0138,

        WM_MOUSEMOVE = 0x0200,
        WM_MOUSEFIRST = WM_MOUSEMOVE,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSEWHEEL = 0x020A,
        WM_XBUTTONDOWN = 0x020B,
        WM_XBUTTONUP = 0x020C,
        WM_XBUTTONDBLCLK = 0x020D,
        WM_MOUSEHWHEEL = 0x020E,
        WM_MOUSELAST = WM_MOUSEHWHEEL,
        WM_PARENTNOTIFY = 0x0210,
        WM_ENTERMENULOOP = 0x0211,
        WM_EXITMENULOOP = 0x0212,
        WM_NEXTMENU = 0x0213,
        WM_SIZING = 0x0214,
        WM_CAPTURECHANGED = 0x0215,
        WM_MOVING = 0x0216,
        WM_POWERBROADCAST = 0x0218,
        WM_DEVICECHANGE = 0x0219,
        WM_POINTERDEVICECHANGE = 0X0238,
        WM_POINTERDEVICEINRANGE = 0x0239,
        WM_POINTERDEVICEOUTOFRANGE = 0x023A,
        WM_POINTERUPDATE = 0x0245,
        WM_POINTERDOWN = 0x0246,
        WM_POINTERUP = 0x0247,
        WM_POINTERENTER = 0x0249,
        WM_POINTERLEAVE = 0x024A,
        WM_POINTERACTIVATE = 0x024B,
        WM_POINTERCAPTURECHANGED = 0x024C,
        WM_IME_SETCONTEXT = 0x0281,
        WM_IME_NOTIFY = 0x0282,
        WM_IME_CONTROL = 0x0283,
        WM_IME_COMPOSITIONFULL = 0x0284,
        WM_IME_SELECT = 0x0285,
        WM_IME_CHAR = 0x0286,
        WM_IME_REQUEST = 0x0288,
        WM_IME_KEYDOWN = 0x0290,
        WM_IME_KEYUP = 0x0291,
        WM_MDICREATE = 0x0220,
        WM_MDIDESTROY = 0x0221,
        WM_MDIACTIVATE = 0x0222,
        WM_MDIRESTORE = 0x0223,
        WM_MDINEXT = 0x0224,
        WM_MDIMAXIMIZE = 0x0225,
        WM_MDITILE = 0x0226,
        WM_MDICASCADE = 0x0227,
        WM_MDIICONARRANGE = 0x0228,
        WM_MDIGETACTIVE = 0x0229,
        WM_MDISETMENU = 0x0230,
        WM_ENTERSIZEMOVE = 0x0231,
        WM_EXITSIZEMOVE = 0x0232,
        WM_DROPFILES = 0x0233,
        WM_MDIREFRESHMENU = 0x0234,
        WM_MOUSEHOVER = 0x02A1,
        WM_NCMOUSELEAVE = 0x02A2,
        WM_MOUSELEAVE = 0x02A3,

        WM_WTSSESSION_CHANGE = 0x02b1,

        WM_TABLET_DEFBASE = 0x02C0,
        WM_DPICHANGED = 0x02E0,
        WM_TABLET_MAXOFFSET = 0x20,

        WM_TABLET_ADDED = WM_TABLET_DEFBASE + 8,
        WM_TABLET_DELETED = WM_TABLET_DEFBASE + 9,
        WM_TABLET_FLICK = WM_TABLET_DEFBASE + 11,
        WM_TABLET_QUERYSYSTEMGESTURESTATUS = WM_TABLET_DEFBASE + 12,

        WM_CUT = 0x0300,
        WM_COPY = 0x0301,
        WM_PASTE = 0x0302,
        WM_CLEAR = 0x0303,
        WM_UNDO = 0x0304,
        WM_RENDERFORMAT = 0x0305,
        WM_RENDERALLFORMATS = 0x0306,
        WM_DESTROYCLIPBOARD = 0x0307,
        WM_DRAWCLIPBOARD = 0x0308,
        WM_PAINTCLIPBOARD = 0x0309,
        WM_VSCROLLCLIPBOARD = 0x030A,
        WM_SIZECLIPBOARD = 0x030B,
        WM_ASKCBFORMATNAME = 0x030C,
        WM_CHANGECBCHAIN = 0x030D,
        WM_HSCROLLCLIPBOARD = 0x030E,
        WM_QUERYNEWPALETTE = 0x030F,
        WM_PALETTEISCHANGING = 0x0310,
        WM_PALETTECHANGED = 0x0311,
        WM_HOTKEY = 0x0312,
        WM_PRINT = 0x0317,
        WM_PRINTCLIENT = 0x0318,
        WM_APPCOMMAND = 0x0319,
        WM_THEMECHANGED = 0x031A,

        WM_DWMCOMPOSITIONCHANGED = 0x031E,
        WM_DWMNCRENDERINGCHANGED = 0x031F,
        WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
        WM_HANDHELDFIRST = 0x0358,
        WM_HANDHELDLAST = 0x035F,
        WM_AFXFIRST = 0x0360,
        WM_AFXLAST = 0x037F,
        WM_PENWINFIRST = 0x0380,
        WM_PENWINLAST = 0x038F,

        #region Windows 7
        WM_DWMSENDICONICTHUMBNAIL = 0x0323,
        WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
        #endregion

        WM_USER = 0x0400,

        WM_APP = 0x8000
    }
}

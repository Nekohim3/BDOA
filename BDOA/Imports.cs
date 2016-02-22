using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using Point = System.Drawing.Point;

namespace BDOA
{
    internal static class Imports
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Point pt);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        internal enum SystemMetric
        {
            SmCxscreen = 0,
            SmCyscreen = 1
        }

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(SystemMetric smIndex);

        internal static int Absx(int x)
        {
            return x*65536/GetSystemMetrics(SystemMetric.SmCxscreen);
        }

        internal static int Absy(int y)
        {
            return y*65536/GetSystemMetrics(SystemMetric.SmCyscreen);
        }

        [DllImport("kernel32")]
        internal static extern bool AllocConsole();

        [Serializable, StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;

            internal RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            internal Rectangle AsRectangle => new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }

        [SuppressUnmanagedCodeSecurity]
        internal sealed class NativeMethods
        {
            [DllImport("user32.dll")]
            internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            internal static Rectangle GetClientRect(IntPtr hwnd)
            {
                RECT rect;
                GetClientRect(hwnd, out rect);
                return rect.AsRectangle;
            }

            internal static Rectangle GetWindowRect(IntPtr hwnd)
            {
                RECT rect;
                GetWindowRect(hwnd, out rect);
                return rect.AsRectangle;
            }

            internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
            {
                var windowRect = GetWindowRect(hWnd);
                var clientRect = GetClientRect(hWnd);
                var chromeWidth = (windowRect.Width - clientRect.Width) / 2;
                return
                    new Rectangle(
                        new Point(windowRect.X + chromeWidth,
                            windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
            }
        }
        
    }
}

using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Wonder.Core.Util;

namespace Wonder.Core.Behaviors
{
    public class ControlWindowBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            var window = Window.GetWindow(AssociatedObject);
            window.SourceInitialized += OnSourceInitialized;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            var window = Window.GetWindow(AssociatedObject);
            window.SourceInitialized -= OnSourceInitialized;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            base.OnDetaching();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !(Window.GetWindow(AssociatedObject) is Window window))
                return;

            window.DragMove();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || !(Window.GetWindow(AssociatedObject) is Window window))
                return;

            window.WindowState = window.WindowState == WindowState.Normal
                                ? WindowState.Maximized
                                : WindowState.Normal;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            //var window = (Window)sender;
            //var hwnd = new WindowInteropHelper(window).Handle;
            //var source = HwndSource.FromHwnd(hwnd);
            var source = (HwndSource)PresentationSource.FromVisual(AssociatedObject);
            source.AddHook(WindowHook);
        }

        private IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var message = (Win32Codes)msg;
            switch (message)
            {
                case Win32Codes.WM_GETMINMAXINFO:
                    handled = HandleWM_GETMINMAXINFO(hwnd, lParam);
                    break;
                case Win32Codes.WM_SETTINGCHANGE:
                    handled = HandleWM_SETTINGCHANGE(hwnd);
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private bool HandleWM_SETTINGCHANGE(IntPtr hwnd)
        {
            // 调整最大化位置和尺寸到对应显示器的工作区域
            var window = Window.GetWindow(AssociatedObject);
            if (window.WindowState != WindowState.Maximized)
                return false;
            var hMonitor = Win32Utils.MonitorFromWindow(hwnd, Win32Utils.MONITOR_DEFAULTTONULL);
            if (hMonitor == IntPtr.Zero)
                return false;
            var mi = new MONITORINFO();
            mi.Size = Marshal.SizeOf(mi);
            if (!Win32Utils.GetMonitorInfo(hMonitor, ref mi))
                return false;
            var mr = mi.MonitorRect;
            var wr = mi.WorkRect;
            var x = Math.Abs(wr.Left - mr.Left);
            var y = Math.Abs(wr.Top - mr.Top);
            var nWidth = Math.Abs(wr.Right - wr.Left);
            var nHeight = Math.Abs(wr.Bottom - wr.Top);
            return Win32Utils.MoveWindow(hwnd, x, y, nWidth, nHeight, false);
        }

        private bool HandleWM_GETMINMAXINFO(IntPtr hwnd, IntPtr lParam)
        {
            // 调整最大化位置和尺寸到对应显示器的工作区域
            var hMonitor = Win32Utils.MonitorFromWindow(hwnd, Win32Utils.MONITOR_DEFAULTTONULL);
            if (hMonitor == IntPtr.Zero)
                return false;
            var mi = new MONITORINFO();
            mi.Size = Marshal.SizeOf(mi);
            if (!Win32Utils.GetMonitorInfo(hMonitor, ref mi))
                return false;
            var mr = mi.MonitorRect;
            var wr = mi.WorkRect;
            // 获取最大最小信息
            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            // 获取最大最小窗口位置和尺寸
            var left = Math.Abs(wr.Left - mr.Left);
            var top = Math.Abs(wr.Top - mr.Top);
            var width = Math.Abs(wr.Right - wr.Left);
            var height = Math.Abs(wr.Bottom - wr.Top);
            var maxPosition = new POINT(left, top);
            var maxSize = new POINT(width, height);
            // 获取最大最小限制尺寸
            var window = Window.GetWindow(AssociatedObject);
            var minLimit = new Point(window.MinWidth, window.MinHeight);
            var maxLimit = new Point(window.MaxWidth, window.MaxHeight);
            //var source = (HwndSource)PresentationSource.FromVisual(AssociatedObject);
            var source = HwndSource.FromHwnd(hwnd);
            var matrix = source.CompositionTarget.TransformToDevice;
            var minUnits = matrix.Transform(minLimit);
            var maxUnits = matrix.Transform(maxLimit);
            var minX = Convert.ToInt32(minUnits.X);
            var minY = Convert.ToInt32(minUnits.Y);
            var maxX = double.IsInfinity(maxUnits.X)
                     ? mmi.MaxTrackSize.X
                     : Convert.ToInt32(maxUnits.X);
            var maxY = double.IsInfinity(maxUnits.Y)
                     ? mmi.MaxTrackSize.Y
                     : Convert.ToInt32(maxUnits.Y);
            var minTrackSize = new POINT(minX, minY);
            var maxTrackSize = new POINT(maxX, maxY);
            // 赋值到最大最小化信息句柄
            mmi.MaxPosition = maxPosition;
            mmi.MaxSize = maxSize;
            mmi.MinTrackSize = minTrackSize;
            mmi.MaxTrackSize = maxTrackSize;
            Marshal.StructureToPtr(mmi, lParam, true);
            return true;
        }
    }
}

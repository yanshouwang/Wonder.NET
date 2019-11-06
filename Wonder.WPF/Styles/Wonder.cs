using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Wonder.WPF.Extension;
using Wonder.WPF.Util;

namespace Wonder.WPF.Styles
{
    public static class Wonder
    {
        #region IsBindingToSystemCommands
        public static bool GetIsBindingToSystemCommands(DependencyObject obj)
            => (bool)obj.GetValue(IsBindingToSystemCommandsProperty);

        public static void SetIsBindingToSystemCommands(DependencyObject obj, bool value)
            => obj.SetValue(IsBindingToSystemCommandsProperty, value);

        // Using a DependencyProperty as the backing store for IsBindingToSystemCommands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBindingToSystemCommandsProperty =
            DependencyProperty.RegisterAttached("IsBindingToSystemCommands", typeof(bool), typeof(Wonder), new PropertyMetadata(default(bool), OnIsBindingToSystemCommandsPropertyChanged));

        private static void OnIsBindingToSystemCommandsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)d;
            var isBindingToSystemCommands = (bool)e.NewValue;
            if (isBindingToSystemCommands)
            {
                var window = element.GetWindow();
                if (window == null)
                    return;
                var minimize = new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow);
                var maximize = new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanMaximizeWindow);
                var restore = new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanRestoreWindow);
                var close = new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow, CanCloseWindow);
                var menu = new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu, CanShowSystemMenu);
                var commands = new[] { minimize, maximize, restore, close, menu };
                element.CommandBindings.AddRange(commands);
            }
            else
            {
                if (element.CommandBindings == null)
                    return;
                for (int i = element.CommandBindings.Count - 1; i >= 0; i--)
                {
                    var command = element.CommandBindings[i].Command;
                    if (command != SystemCommands.MinimizeWindowCommand &&
                        command != SystemCommands.MaximizeWindowCommand &&
                        command != SystemCommands.RestoreWindowCommand &&
                        command != SystemCommands.CloseWindowCommand &&
                        command != SystemCommands.ShowSystemMenuCommand)
                    {
                        continue;
                    }
                    element.CommandBindings.RemoveAt(i);
                }
            }
        }

        private static void CanShowSystemMenu(object sender, CanExecuteRoutedEventArgs e)
        {
            var value = sender is UIElement element && element.GetWindow() != null;
            e.CanExecute = value;
        }

        private static void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) ||
                !(element.GetWindow() is Window window))
                return;
            var location = Mouse.GetPosition(window);
            location = window.PointToScreen(location);
            SystemCommands.ShowSystemMenu(window, location);
        }

        private static void CanCloseWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var value = sender is UIElement element && element.GetWindow() != null;
            e.CanExecute = value;
        }

        private static void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) ||
                !(element.GetWindow() is Window window))
                return;
            SystemCommands.CloseWindow(window);
        }

        private static void CanRestoreWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var value = sender is UIElement element &&
                        element.GetWindow() is Window window &&
                        window.ResizeMode != ResizeMode.NoResize &&
                        window.ResizeMode != ResizeMode.CanMinimize;
            e.CanExecute = value;
        }

        private static void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) ||
                !(element.GetWindow() is Window window))
                return;
            SystemCommands.RestoreWindow(window);
        }

        private static void CanMaximizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var value = sender is UIElement element &&
                        element.GetWindow() is Window window &&
                        window.ResizeMode != ResizeMode.NoResize &&
                        window.ResizeMode != ResizeMode.CanMinimize;
            e.CanExecute = value;
        }

        private static void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) ||
                !(element.GetWindow() is Window window))
                return;
            SystemCommands.MaximizeWindow(window);
        }

        private static void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            var value = sender is UIElement element &&
                        element.GetWindow() is Window window &&
                        window.ResizeMode != ResizeMode.NoResize;
            e.CanExecute = value;
        }

        private static void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) ||
                !(element.GetWindow() is Window window))
                return;
            SystemCommands.MinimizeWindow(window);
        }
        #endregion

        #region IsMaximizedToWrokAera
        public static bool GetIsMaximizedToWorkAera(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMaximizedToWorkAeraProperty);
        }

        public static void SetIsMaximizedToWorkAera(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMaximizedToWorkAeraProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsMaximizedToWorkAera.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximizedToWorkAeraProperty =
            DependencyProperty.RegisterAttached("IsMaximizedToWorkAera", typeof(bool), typeof(Wonder), new PropertyMetadata(default(bool), OnIsMaximizedToWorkAeraPropertyChanged));

        private static void OnIsMaximizedToWorkAeraPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (Window)d;
            window.SourceInitialized += OnSourceInitialized;
        }

        private static void OnSourceInitialized(object sender, EventArgs e)
        {
            var window = (Window)sender;
            var source = (HwndSource)PresentationSource.FromVisual(window);
            source.AddHook(WindowHook);
        }

        private static IntPtr WindowHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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

        private static bool HandleWM_SETTINGCHANGE(IntPtr hwnd)
        {
            var source = HwndSource.FromHwnd(hwnd);
            var window = (Window)source.RootVisual;
            var isMaximizedToWorkArea = GetIsMaximizedToWorkAera(window);
            // 调整最大化位置和尺寸到对应显示器的工作区域
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
            var x = mr.Left;
            var y = mr.Top;
            var nWidth = mr.Width;
            var nHeight = mr.Height;
            if (isMaximizedToWorkArea)
            {
                x = Math.Abs(wr.Left - mr.Left);
                y = Math.Abs(wr.Top - mr.Top);
                nWidth = Math.Abs(wr.Right - wr.Left);
                nHeight = Math.Abs(wr.Bottom - wr.Top);
            }
            return Win32Utils.MoveWindow(hwnd, x, y, nWidth, nHeight, false);
        }

        private static bool HandleWM_GETMINMAXINFO(IntPtr hwnd, IntPtr lParam)
        {
            var source = HwndSource.FromHwnd(hwnd);
            var window = (Window)source.RootVisual;
            var isMaximizedToWorkArea = GetIsMaximizedToWorkAera(window);
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
            var left = mr.Left;
            var top = mr.Top;
            var width = mr.Width;
            var height = mr.Height;
            if (isMaximizedToWorkArea)
            {
                left = Math.Abs(wr.Left - mr.Left);
                top = Math.Abs(wr.Top - mr.Top);
                width = Math.Abs(wr.Right - wr.Left);
                height = Math.Abs(wr.Bottom - wr.Top);
            }
            var maxPosition = new POINT(left, top);
            var maxSize = new POINT(width, height);
            // 获取最大最小限制尺寸
            var minLimit = new Point(window.MinWidth, window.MinHeight);
            var maxLimit = new Point(window.MaxWidth, window.MaxHeight);
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
        #endregion

        #region Geometry
        public static Geometry GetGeometry(DependencyObject obj)
            => (Geometry)obj.GetValue(GeometryProperty);

        public static void SetGeometry(DependencyObject obj, Geometry value)
            => obj.SetValue(GeometryProperty, value);

        // Using a DependencyProperty as the backing store for Geometry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.RegisterAttached("Geometry", typeof(Geometry), typeof(Wonder), new PropertyMetadata(default(Geometry)));
        #endregion

        public static Brush GetMouseOverBackground(DependencyObject obj)
            => (Brush)obj.GetValue(MouseOverBackgroundProperty);

        public static void SetMouseOverBackground(DependencyObject obj, Brush value)
            => obj.SetValue(MouseOverBackgroundProperty, value);

        // Using a DependencyProperty as the backing store for MouseOverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static Brush GetMouseOverForeground(DependencyObject obj)
            => (Brush)obj.GetValue(MouseOverForegroundProperty);

        public static void SetMouseOverForeground(DependencyObject obj, Brush value)
            => obj.SetValue(MouseOverForegroundProperty, value);

        // Using a DependencyProperty as the backing store for MouseOverForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.RegisterAttached("MouseOverForeground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static Brush GetPressedBackground(DependencyObject obj)
            => (Brush)obj.GetValue(PressedBackgroundProperty);

        public static void SetPressedBackground(DependencyObject obj, Brush value)
            => obj.SetValue(PressedBackgroundProperty, value);

        // Using a DependencyProperty as the backing store for PressedBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.RegisterAttached("PressedBackground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static Brush GetPressedForeground(DependencyObject obj)
            => (Brush)obj.GetValue(PressedForegroundProperty);

        public static void SetPressedForeground(DependencyObject obj, Brush value)
            => obj.SetValue(PressedForegroundProperty, value);

        // Using a DependencyProperty as the backing store for PressedForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.RegisterAttached("PressedForeground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static Brush GetDisabledBackground(DependencyObject obj)
            => (Brush)obj.GetValue(DisabledBackgroundProperty);

        public static void SetDisabledBackground(DependencyObject obj, Brush value)
            => obj.SetValue(DisabledBackgroundProperty, value);

        // Using a DependencyProperty as the backing store for DisabledBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.RegisterAttached("DisabledBackground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static Brush GetDisabledForeground(DependencyObject obj)
            => (Brush)obj.GetValue(DisabledForegroundProperty);

        public static void SetDisabledForeground(DependencyObject obj, Brush value)
            => obj.SetValue(DisabledForegroundProperty, value);

        // Using a DependencyProperty as the backing store for DisabledForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.RegisterAttached("DisabledForeground", typeof(Brush), typeof(Wonder), new PropertyMetadata(default(Brush)));

        public static bool GetIsWindowActive(DependencyObject obj)
            => (bool)obj.GetValue(IsWindowActiveProperty);

        public static void SetIsWindowActive(DependencyObject obj, bool value)
            => obj.SetValue(IsWindowActiveProperty, value);

        // Using a DependencyProperty as the backing store for IsWindowActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsWindowActiveProperty =
            DependencyProperty.RegisterAttached("IsWindowActive", typeof(bool), typeof(Wonder), new PropertyMetadata(true));
    }
}

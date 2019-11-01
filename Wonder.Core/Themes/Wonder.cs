using System.Windows;
using System.Windows.Input;
using Wonder.Core.Extension;

namespace Wonder.Core.Themes
{
    public static class Wonder
    {
        public static bool GetIsBindingToSystemCommands(DependencyObject obj)
            => (bool)obj.GetValue(IsBindingToSystemCommandsProperty);

        public static void SetIsBindingToSystemCommands(DependencyObject obj, bool value)
            => obj.SetValue(IsBindingToSystemCommandsProperty, value);

        // Using a DependencyProperty as the backing store for IsBindingToSystemCommands.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBindingToSystemCommandsProperty =
            DependencyProperty.RegisterAttached("IsBindingToSystemCommands", typeof(bool), typeof(Wonder), new PropertyMetadata(default(bool), OnIsBindingToSystemCommandsPropertyChanged));

        private static void OnIsBindingToSystemCommandsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
                return;
            var value = (bool)e.NewValue;
            if (!value)
                return;
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
    }
}

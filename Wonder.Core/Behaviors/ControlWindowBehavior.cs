using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace Wonder.Core.Behaviors
{
    public class ControlWindowBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || !(Window.GetWindow(AssociatedObject) is Window window))
                return;

            window.WindowState = window.WindowState == WindowState.Normal
                                ? WindowState.Maximized
                                : WindowState.Normal;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !(Window.GetWindow(AssociatedObject) is Window window))
                return;

            window.DragMove();
        }
    }
}

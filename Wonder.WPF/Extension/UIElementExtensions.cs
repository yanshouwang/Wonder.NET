using System.Windows;

namespace Wonder.WPF.Extension
{
    public static class UIElementExtensions
    {
        public static Window GetWindow(this UIElement element)
        {
            var window = element is Window w1
                       ? w1
                       : Window.GetWindow(element) is Window w2
                       ? w2
                       : null;
            return window;
        }
    }
}

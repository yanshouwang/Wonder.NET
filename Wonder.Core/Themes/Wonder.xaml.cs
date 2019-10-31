using System.Windows;

namespace Wonder.Core.Themes
{
    public partial class Wonder
    {
        public static object GetTitleBar(DependencyObject obj)
            => (object)obj.GetValue(TitleBarProperty);

        public static void SetTitleBar(DependencyObject obj, object value)
            => obj.SetValue(TitleBarProperty, value);

        // Using a DependencyProperty as the backing store for TitleBar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleBarProperty =
            DependencyProperty.RegisterAttached("TitleBar", typeof(object), typeof(Window), new PropertyMetadata(null));
    }
}

using Windows.UI.Xaml;

namespace Wonder.UWP.Helpers
{
    public static class NavHelper
    {
        public static string GetViewToken(DependencyObject obj)
            => (string)obj.GetValue(ViewTokenProperty);

        public static void SetViewToken(DependencyObject obj, string value)
            => obj.SetValue(ViewTokenProperty, value);

        /// <summary>
        /// Using a DependencyProperty as the backing store for ViewToken.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ViewTokenProperty =
            DependencyProperty.RegisterAttached(
                "ViewToken",
                typeof(string),
                typeof(NavHelper),
                new PropertyMetadata(null));
    }
}

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Wonder.UWP.Extension
{
    public static class FrameworkElementExtensions
    {
        public static FrameworkElement GetChild(this FrameworkElement element, string name)
        {
            var target = element;
            if (target.Name == name)
                return target;
            var count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; i++)
            {
                if (VisualTreeHelper.GetChild(element, i) is FrameworkElement child)
                {
                    target = GetChild(child, name);
                    if (target != null)
                        return target;
                }
            }
            return null;
        }
    }
}

using System.Linq;
using System.Windows;

namespace Wonder.WPF
{
    public static class Extensions
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

        public static string LoopInsert(this string str, int interval, string value)
        {
            for (int i = interval; i < str.Length; i += interval + 1)
                str = str.Insert(i, value);
            return str;
        }
    }
}

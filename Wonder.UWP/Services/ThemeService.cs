using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Wonder.UWP.Constants;

namespace Wonder.UWP.Services
{
    public static class ThemeService
    {
        private static UISettings _uiSettings = new UISettings();

        public static void SetTheme(ElementTheme theme)
        {
            // 设置标题栏按钮主题
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var target = theme != ElementTheme.Default
                       ? theme
                       : Application.Current.RequestedTheme == ApplicationTheme.Light
                       ? ElementTheme.Light
                       : ElementTheme.Dark;
            if (target == ElementTheme.Light)
            {
                titleBar.ButtonForegroundColor = Colors.Black;
            }
            else
            {
                titleBar.ButtonForegroundColor = Colors.White;
            }
            // TODO: 当在系统中切换主题时，标题栏按钮颜色不生效
            if (theme == ElementTheme.Default)
            {
                _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            }
            else
            {
                _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
            }
            // 设置元素主题
            if (!(Window.Current.Content is FrameworkElement element))
                return;
            element.RequestedTheme = theme;
        }

        private static void OnColorValuesChanged(UISettings sender, object args)
        {

        }
    }
}

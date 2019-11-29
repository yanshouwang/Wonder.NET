using Microsoft.Practices.Unity;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Wonder.UWP.Extension;
using Wonder.UWP.Xaml;
using MTUH = Microsoft.Toolkit.Uwp.Helpers;

namespace Wonder.UWP.Services
{
    public class ThemeService : IThemeService
    {
        public IUnityContainer Container { get; }
        public UISettings UISettings { get; }

        public ThemeService(IUnityContainer container)
        {
            Container = container;
            UISettings = new UISettings();
        }

        public void SetThemeMode(ThemeMode mode)
        {
            if (!(Window.Current.Content is FrameworkElement element))
                return;
            var theme = mode == ThemeMode.Light
                      ? ElementTheme.Light
                      : mode == ThemeMode.Dark
                      ? ElementTheme.Dark
                      : ElementTheme.Default;
            element.RequestedTheme = theme;
            UpdateTitleBar(theme);
            if (theme == ElementTheme.Default)
            {
                UISettings.ColorValuesChanged += OnColorValuesChanged;
            }
            else
            {
                UISettings.ColorValuesChanged -= OnColorValuesChanged;
            }
        }

        private void UpdateTitleBar(ElementTheme theme)
        {
            var titleBar = Container.Resolve<ApplicationViewTitleBar>();
            if (theme == ElementTheme.Default)
            {
                theme = UISettings.GetColorValue(UIColorType.Background) == Colors.White
                      ? ElementTheme.Light
                      : ElementTheme.Dark;
            }
            if (theme == ElementTheme.Light)
            {
                if (titleBar.ButtonForegroundColor == Colors.Black)
                    return;
                titleBar.ButtonHoverBackgroundColor = MTUH.ColorHelper.ToColor("#19000000");
                titleBar.ButtonPressedBackgroundColor = MTUH.ColorHelper.ToColor("#33000000");
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonInactiveForegroundColor = MTUH.ColorHelper.ToColor("#FF858585");
            }
            else
            {
                if (titleBar.ButtonForegroundColor == Colors.White)
                    return;
                titleBar.ButtonHoverBackgroundColor = MTUH.ColorHelper.ToColor("#19FFFFFF");
                titleBar.ButtonPressedBackgroundColor = MTUH.ColorHelper.ToColor("#33FFFFFF");
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonInactiveForegroundColor = MTUH.ColorHelper.ToColor("#FF6D6D6D");
            }
        }

        private void OnColorValuesChanged(UISettings sender, object args)
        {
            UpdateTitleBar(ElementTheme.Default);
        }

        public void SetThemeColor(Color themeColor)
        {
            var themeScale = themeColor.GetColorScale();
            var light1 = themeScale.GetColor(0.4);
            var light2 = themeScale.GetColor(0.3);
            var light3 = themeScale.GetColor(0.2);
            var dark1 = themeScale.GetColor(0.6);
            var dark2 = themeScale.GetColor(0.7);
            var dark3 = themeScale.GetColor(0.8);
            Application.Current.Resources["SystemAccentColor"] = themeColor;
            Application.Current.Resources["SystemAccentColorLight1"] = light1;
            Application.Current.Resources["SystemAccentColorLight2"] = light2;
            Application.Current.Resources["SystemAccentColorLight3"] = light3;
            Application.Current.Resources["SystemAccentColorDark1"] = dark1;
            Application.Current.Resources["SystemAccentColorDark2"] = dark2;
            Application.Current.Resources["SystemAccentColorDark3"] = dark3;
            ReloadTheme();
        }

        private void ReloadTheme()
        {
            if (!(Window.Current.Content is FrameworkElement element))
                return;
            var current = element.RequestedTheme;
            var theme = current == ElementTheme.Light
                      ? ElementTheme.Dark
                      : current == ElementTheme.Dark
                      ? ElementTheme.Light
                      : UISettings.GetColorValue(UIColorType.Background) == Colors.White
                      ? ElementTheme.Dark
                      : ElementTheme.Light;
            element.RequestedTheme = theme;
            element.RequestedTheme = current;
        }
    }
}

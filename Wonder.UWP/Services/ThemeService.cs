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
    public class ThemeService
    {
        public IUnityContainer Container { get; }
        public UISettings UISettings { get; }

        public ThemeService(IUnityContainer container, UISettings uiSettings)
        {
            Container = container;
            UISettings = uiSettings;
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
            //var resources0 = FindColorPaletteResourcesForTheme("Default");
            //var resources1 = FindColorPaletteResourcesForTheme("Light");
            //resources0.Accent = resources1.Accent = themeColor;

            Application.Current.Resources["SystemAccentColor"] = themeColor;
            var themeScale = themeColor.GetPaletteScale();
            var light1 = themeScale.GetPaletteColor(4, 11);
            var light2 = themeScale.GetPaletteColor(3, 11);
            var light3 = themeScale.GetPaletteColor(2, 11);
            var dark1 = themeScale.GetPaletteColor(6, 11);
            var dark2 = themeScale.GetPaletteColor(7, 11);
            var dark3 = themeScale.GetPaletteColor(8, 11);
            Application.Current.Resources["SystemAccentColorLight1"] = light1;
            Application.Current.Resources["SystemAccentColorLight2"] = light2;
            Application.Current.Resources["SystemAccentColorLight3"] = light3;
            Application.Current.Resources["SystemAccentColorDark1"] = dark1;
            Application.Current.Resources["SystemAccentColorDark2"] = dark2;
            Application.Current.Resources["SystemAccentColorDark3"] = dark3;
            ReloadThemeColor(themeColor);
        }

        private void ReloadThemeColor(Color themeColor)
        {
            // 需要设置 LIGHT1 - DARK3 主题颜色，且深色和浅色主题模式均需要设置
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

        private ColorPaletteResources FindColorPaletteResourcesForTheme(string theme)
        {
            var dictionary = Application.Current.Resources.MergedDictionaries.Single(i => i.Source.AbsoluteUri == "ms-resource:///Files/Xaml/ThemeColors.xaml");
            foreach (var themeDictionary in dictionary.ThemeDictionaries)
            {
                if (themeDictionary.Key.ToString() == theme)
                {
                    if (themeDictionary.Value is ColorPaletteResources)
                    {
                        return themeDictionary.Value as ColorPaletteResources;
                    }
                    else if (themeDictionary.Value is ResourceDictionary targetDictionary)
                    {
                        foreach (var mergedDictionary in targetDictionary.MergedDictionaries)
                        {
                            if (mergedDictionary is ColorPaletteResources)
                            {
                                return mergedDictionary as ColorPaletteResources;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}

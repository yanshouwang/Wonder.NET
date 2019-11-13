using Microsoft.Practices.Unity;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Wonder.UWP.Xaml;
using MTUH = Microsoft.Toolkit.Uwp.Helpers;
using MUXM = Microsoft.UI.Xaml.Media;

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

        public void SetThemeColor(Color color)
        {
            //var brush1 = Application.Current.Resources["SystemControlHighlightAccentRevealBorderBrush"] as MUXM.RevealBorderBrush;
            //brush1.Color = brush1.FallbackColor = color;
            //(Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = color;
            //var value = new ResourceDictionary();
            //value.Source = new Uri("ms-appx:///Xaml/Forest.xaml");
            //Application.Current.Resources["SystemAccentColor"] = color;
            var light = FindColorPaletteResourcesForTheme("Light");
            var dark = FindColorPaletteResourcesForTheme("Default");
            light.Accent = color;
            dark.Accent = color;
            ReloadThemeResources();
        }

        private void ReloadThemeResources()
        {
            if (!(Window.Current.Content is FrameworkElement element))
                return;
            var current = element.RequestedTheme;
            var theme = current == ElementTheme.Light
                      ? ElementTheme.Dark
                      : ElementTheme.Light;
            element.RequestedTheme = theme;
            element.RequestedTheme = current;
        }

        private ColorPaletteResources FindColorPaletteResourcesForTheme(string theme)
        {
            foreach (var themeDictionary in Application.Current.Resources.ThemeDictionaries)
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

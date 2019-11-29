using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Wonder.UWP.Constants;
using Wonder.UWP.Services;
using Wonder.UWP.Xaml;

namespace Wonder.UWP.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public IThemeService ThemeService { get; }

        private ThemeMode _themeMode;
        public ThemeMode ThemeMode
        {
            get { return _themeMode; }
            set { SetProperty(ref _themeMode, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        private Version _version;
        public Version Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        public SettingsViewModel(INavigationService navigationService, IThemeService themeService)
            : base(navigationService)
        {
            ThemeService = themeService;
            ThemeMode = ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsKeys.THEME)
                      ? (ThemeMode)ApplicationData.Current.LocalSettings.Values[SettingsKeys.THEME]
                      : ThemeMode.System;
            DisplayName = GetDisplayName();
            Version = GetVersion();
        }

        private Version GetVersion()
        {
            var value = Package.Current.Id.Version;
            var version = new Version(value.Major, value.Minor, value.Build, value.Revision);
            return version;
        }

        private string GetDisplayName()
        {
            var loader = new ResourceLoader();
            var displayName = loader.GetString("DisplayName");
            return displayName;
        }

        private DelegateCommand<object> _setThemeModeCommand;
        public DelegateCommand<object> SetThemeModeCommand =>
            _setThemeModeCommand ?? (_setThemeModeCommand = new DelegateCommand<object>(ExecuteSetThemeModeCommand));

        void ExecuteSetThemeModeCommand(object obj)
        {
            var mode = (ThemeMode)obj;
            ThemeService.SetThemeMode(mode);
            ThemeMode = mode;
            ApplicationData.Current.LocalSettings.Values[SettingsKeys.THEME] = (int)mode;
            var properties = new Dictionary<string, string> { ["ThemeMode"] = mode.ToString() };
            Analytics.TrackEvent("Settings", properties);
        }

        private DelegateCommand<object> _setThemeColorCommand;
        public DelegateCommand<object> SetThemeColorCommand =>
            _setThemeColorCommand ?? (_setThemeColorCommand = new DelegateCommand<object>(ExecuteSetThemeColorCommand));

        void ExecuteSetThemeColorCommand(object obj)
        {
            var color = (Color)obj;
            ThemeService.SetThemeColor(color);
            var properties = new Dictionary<string, string> { ["ThemeColor"] = color.ToString() };
            Analytics.TrackEvent("Settings", properties);
        }
    }
}

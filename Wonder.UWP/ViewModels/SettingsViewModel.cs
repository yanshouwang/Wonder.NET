using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Wonder.UWP.Constants;
using Wonder.UWP.Services;
using Wonder.UWP.Xaml;

namespace Wonder.UWP.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ThemeService ThemeService { get; }

        private ThemeMode _themeMode;
        public ThemeMode ThemeMode
        {
            get { return _themeMode; }
            set { SetProperty(ref _themeMode, value); }
        }

        public SettingsViewModel(INavigationService navigationService, ThemeService themeService)
            : base(navigationService)
        {
            ThemeService = themeService;
            ThemeMode = ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsKeys.THEME)
                      ? (ThemeMode)ApplicationData.Current.LocalSettings.Values[SettingsKeys.THEME]
                      : ThemeMode.System;
        }

        private DelegateCommand<object> _setThemeCommand;
        public DelegateCommand<object> SetThemeCommand =>
            _setThemeCommand ?? (_setThemeCommand = new DelegateCommand<object>(ExecuteSetThemeCommand));

        void ExecuteSetThemeCommand(object obj)
        {
            var mode = (ThemeMode)obj;
            ThemeService.SetThemeMode(mode);
            ThemeMode = mode;
            ApplicationData.Current.LocalSettings.Values[SettingsKeys.THEME] = (int)mode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.UI.Xaml;
using Wonder.UWP.Services;

namespace Wonder.UWP.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }

        private DelegateCommand<object> _setThemeCommand;
        public DelegateCommand<object> SetThemeCommand =>
            _setThemeCommand ?? (_setThemeCommand = new DelegateCommand<object>(ExecuteSetThemeCommand));

        void ExecuteSetThemeCommand(object obj)
        {
            var theme = (ElementTheme)obj;
            ThemeService.SetTheme(theme);
        }
    }
}

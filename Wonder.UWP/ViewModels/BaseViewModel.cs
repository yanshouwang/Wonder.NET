using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Wonder.UWP.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
        protected INavigationService NavigationService { get; }

        public BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public async void RunOnUI(DispatchedHandler handler)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                handler);
        }
    }
}

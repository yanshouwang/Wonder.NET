using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

namespace Wonder.UWP.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase
    {
        protected INavigationService NavigationService { get; }

        protected BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected static IAsyncAction DispatcherRunAsync(Action action)
        {
            var handler = new DispatchedHandler(action);
            return CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                handler);
        }
    }
}

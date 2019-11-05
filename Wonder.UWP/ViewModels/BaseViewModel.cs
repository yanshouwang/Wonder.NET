using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

namespace Wonder.UWP.ViewModels
{
    public class BaseViewModel : ViewModelBase
    {
        protected INavigationService NavigationService { get; }

        public BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }
    }
}

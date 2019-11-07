using Prism.Commands;
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

        private DelegateCommand _gobackCommand;
        public DelegateCommand GoBackCommand =>
            _gobackCommand ?? (_gobackCommand = new DelegateCommand(ExecuteGoBackCommand));

        void ExecuteGoBackCommand()
        {
            if (!NavigationService.CanGoBack())
                return;

            NavigationService.GoBack();
        }
    }
}

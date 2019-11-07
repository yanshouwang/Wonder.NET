using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.Generic;

namespace Wonder.UWP.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private IList<string> _views;
        public IList<string> Views
        {
            get { return _views; }
            set { SetProperty(ref _views, value); }
        }

        public ShellViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Views = new List<string>() { "ABC" };
        }

        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(string viewToken)
        {
            NavigationService.Navigate(viewToken, null);
        }
    }
}

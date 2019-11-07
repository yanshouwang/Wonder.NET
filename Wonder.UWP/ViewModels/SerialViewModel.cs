using Prism.Windows.Navigation;

namespace Wonder.UWP.ViewModels
{
    public class SerialViewModel : BaseViewModel
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public SerialViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Serial";
        }
    }
}

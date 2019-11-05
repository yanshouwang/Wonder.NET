using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Navigation;

namespace Wonder.UWP.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public HomeViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "HOME";
        }
    }
}

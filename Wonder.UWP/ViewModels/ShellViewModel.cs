using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Wonder.UWP.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        protected IUnityContainer Container { get; }

        public ShellViewModel(INavigationService navigationService, IUnityContainer container)
            : base(navigationService)
        {
            Container = container;
            CustomizeTitleBar();
        }

        private void CustomizeTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            // 注入标题栏，因为程序处于后台时无法获取
            Container.RegisterInstance(viewTitleBar);
            // 扩展视图至标题栏区域
            coreTitleBar.ExtendViewIntoTitleBar = true;
            // 自定义标题栏颜色
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(string viewToken)
        {
            if (NavigationService.Navigate(viewToken, null))
                return;

            throw new NotImplementedException();
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

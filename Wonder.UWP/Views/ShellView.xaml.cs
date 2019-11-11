using Microsoft.Practices.Unity;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Wonder.UWP.Helpers;
using Wonder.UWP.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Wonder.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShellView : Page
    {
        public ShellViewModel ViewModel
            => DataContext as ShellViewModel;

        public IUnityContainer Container { get; }
        public Frame NavFrame { get; }

        public ShellView(IUnityContainer container, Frame navFrame)
        {
            this.InitializeComponent();

            Container = container;

            NavFrame = navFrame;
            NavFrame.Navigated += OnNavFrameNavigated;
            NavFrame.NavigationFailed += OnNavFrameNaviagtionFailed;
            NavFrame.NavigationStopped += OnNavFrameNavigationStopped;

            NavView.ItemInvoked += OnNavViewItemInvoked;
            NavView.RegisterPropertyChangedCallback(NavigationView.DisplayModeProperty, OnNavViewPropertyChanged);
            NavView.RegisterPropertyChangedCallback(NavigationView.IsBackButtonVisibleProperty, OnNavViewPropertyChanged);

            InitializeTitleBar();
        }

        private void OnNavViewPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == NavigationView.DisplayModeProperty ||
                dp == NavigationView.IsBackButtonVisibleProperty)
            {
                CalculateTitleBarSize();
            }
        }

        private void OnNavViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var viewToken = args.IsSettingsInvoked
                          ? "SettingsView"
                          : NavHelper.GetViewToken(args.InvokedItemContainer);
            // 判断是否为重复导航
            if (NavFrame.SourcePageType.Name == viewToken ||
                !ViewModel.NavigateCommand.CanExecute(viewToken))
                return;
            ViewModel.NavigateCommand.Execute(viewToken);
        }

        private void OnNavFrameNavigationStopped(object sender, NavigationEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnNavFrameNaviagtionFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnNavFrameNavigated(object sender, NavigationEventArgs e)
        {
            var frame = (Frame)sender;
            // 更新后退按钮状态
            NavView.IsBackEnabled = frame.CanGoBack;
            NavView.IsBackButtonVisible = frame.CanGoBack
                                        ? NavigationViewBackButtonVisible.Visible
                                        : NavigationViewBackButtonVisible.Collapsed;
            // 更新导航栏选择项
            var item = e.SourcePageType.Name == nameof(SettingsView)
                     ? NavView.SettingsItem
                     : NavView.MenuItems.OfType<NavigationViewItem>()
                                        .Single(i => NavHelper.GetViewToken(i) == e.SourcePageType.Name);
            NavView.SelectedItem = item;
            // 更新导航栏标头
            NavView.Header = ((NavigationViewItem)NavView.SelectedItem).Content;
        }

        private void InitializeTitleBar()
        {
            // 扩展视图至标题栏区域
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            // 自定义标题栏颜色
            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            Container.RegisterInstance(viewTitleBar);
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            // 设置自定义标题栏
            Window.Current.SetTitleBar(TitleBar);
            CalculateTitleBarSize();
        }

        private void CalculateTitleBarSize()
        {
            TitleBar.Margin = NavView.DisplayMode == NavigationViewDisplayMode.Minimal &&
                              NavView.IsBackButtonVisible == NavigationViewBackButtonVisible.Visible
                            ? new Thickness(80, 0, 0, 0)
                            : new Thickness(40, 0, 0, 0);
        }
    }
}

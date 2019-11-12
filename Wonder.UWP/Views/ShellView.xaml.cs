using Microsoft.Practices.Unity;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Wonder.UWP.Helpers;
using Wonder.UWP.ViewModels;

using MUXC = Microsoft.UI.Xaml.Controls;

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

        public Frame NavFrame { get; }

        public ShellView(Frame navFrame)
        {
            this.InitializeComponent();

            NavFrame = navFrame;
            NavFrame.Navigated += OnNavFrameNavigated;
            NavFrame.NavigationFailed += OnNavFrameNaviagtionFailed;
            NavFrame.NavigationStopped += OnNavFrameNavigationStopped;

            NavView.ItemInvoked += OnNavViewItemInvoked;
            NavView.RegisterPropertyChangedCallback(MUXC.NavigationView.DisplayModeProperty, OnNavViewPropertyChanged);
            NavView.RegisterPropertyChangedCallback(MUXC.NavigationView.IsBackButtonVisibleProperty, OnNavViewPropertyChanged);

            // 设置自定义标题栏
            Window.Current.SetTitleBar(TitleBar);
            CalculateTitleBarSize();
        }

        private void OnNavViewPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == MUXC.NavigationView.DisplayModeProperty ||
                dp == MUXC.NavigationView.IsBackButtonVisibleProperty)
            {
                CalculateTitleBarSize();
            }
        }

        private void OnNavViewItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
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
                                        ? MUXC.NavigationViewBackButtonVisible.Visible
                                        : MUXC.NavigationViewBackButtonVisible.Collapsed;
            // 更新导航栏选择项
            var item = e.SourcePageType.Name == nameof(SettingsView)
                     ? NavView.SettingsItem
                     : NavView.MenuItems.OfType<MUXC.NavigationViewItem>()
                                        .Single(i => NavHelper.GetViewToken(i) == e.SourcePageType.Name);
            NavView.SelectedItem = item;
            // 更新导航栏标头
            NavView.Header = ((MUXC.NavigationViewItem)NavView.SelectedItem).Content;
        }

        private void CalculateTitleBarSize()
        {
            TitleBar.Margin = NavView.DisplayMode == MUXC.NavigationViewDisplayMode.Minimal &&
                              NavView.IsBackButtonVisible == MUXC.NavigationViewBackButtonVisible.Visible
                            ? new Thickness(80, 0, 0, 0)
                            : new Thickness(40, 0, 0, 0);
        }
    }
}

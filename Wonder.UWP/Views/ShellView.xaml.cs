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

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ExtendTitleBar();
        }

        private void ExtendTitleBar()
        {
            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            // 扩展视图至标题栏区域
            titleBar.ExtendViewIntoTitleBar = true;
            titleBar.LayoutMetricsChanged += OnTitleBarLayoutMetricsChanged;
            // 设置自定义标题栏
            Window.Current.SetTitleBar(TitleBar);
        }

        private void OnTitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                HeaderSpace.MinWidth = sender.SystemOverlayLeftInset;
                FooterSpace.MinWidth = sender.SystemOverlayRightInset;
            }
            else
            {
                HeaderSpace.MinWidth = sender.SystemOverlayRightInset;
                FooterSpace.MinWidth = sender.SystemOverlayLeftInset;
            }
            TabStripHeader.Height = TabStripFooter.Height = sender.Height;
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

        }
    }
}

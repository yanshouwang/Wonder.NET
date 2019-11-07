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

        public ShellView()
        {
            this.InitializeComponent();
            this.CustomizeTitleBar();
            
            NavView.RegisterPropertyChangedCallback(MUXC.NavigationView.DisplayModeProperty, OnNavViewPropertyChanged);
            NavView.RegisterPropertyChangedCallback(MUXC.NavigationView.IsBackButtonVisibleProperty, OnNavViewPropertyChanged);

            CalculateTitleBarPosition();
        }

        private void OnNavViewPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == MUXC.NavigationView.DisplayModeProperty ||
                dp == MUXC.NavigationView.IsBackButtonVisibleProperty)
            {
                CalculateTitleBarPosition();
            }
        }

        private void CalculateTitleBarPosition()
        {
            TitleBar.Margin = NavView.DisplayMode == MUXC.NavigationViewDisplayMode.Minimal &&
                              NavView.IsBackButtonVisible == MUXC.NavigationViewBackButtonVisible.Visible
                            ? new Thickness(80, 0, 0, 0)
                            : new Thickness(40, 0, 0, 0);
        }

        private void CustomizeTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            Window.Current.SetTitleBar(TitleBar);
        }

        public void SetRootFrame(Frame frame)
        {
            NavView.Content = frame;
            frame.Navigated += OnRootFrameNavigated;
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            var frame = (Frame)sender;
            // 更新后退按钮状态
            NavView.IsBackEnabled = frame.CanGoBack;
            NavView.IsBackButtonVisible = frame.CanGoBack
                                        ? MUXC.NavigationViewBackButtonVisible.Visible
                                        : MUXC.NavigationViewBackButtonVisible.Collapsed;
            // 更新导航栏选择项
            if (e.SourcePageType.Name == nameof(SettingsView))
            {
                NavView.SelectedItem = NavView.SettingsItem;
                return;
            }
            var item = NavView.MenuItems
                .OfType<MUXC.NavigationViewItem>()
                .Single(i => NavHelper.GetViewToken(i) == e.SourcePageType.Name);
            item.IsSelected = true;
        }
    }
}

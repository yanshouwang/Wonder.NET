﻿using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Practices.Unity;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wonder.UWP.Constants;
using Wonder.UWP.Services;
using Wonder.UWP.Views;
using Wonder.UWP.Xaml;

namespace Wonder.UWP
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            //this.Suspending += OnSuspending;
            AppCenter.Start("", typeof(Analytics), typeof(Crashes));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance(Container);
            var themeService = Container.Resolve<ThemeService>();
            Container.RegisterInstance(themeService);
        }

        protected override IDeviceGestureService OnCreateDeviceGestureService()
        {
            var service = base.OnCreateDeviceGestureService();
            service.UseTitleBarBackButton = false;
            return service;
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            return base.OnInitializeAsync(args);
        }

        protected override Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            return base.OnActivateApplicationAsync(args);
        }

        protected override Task OnResumeApplicationAsync(IActivatedEventArgs args)
        {
            return base.OnResumeApplicationAsync(args);
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var parameter = new ParameterOverride("navFrame", rootFrame);
            var shell = Container.Resolve<ShellView>(parameter);
            return shell;
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            InitializeTheme();
            NavigationService.Navigate(ViewTokens.SETTINGS, null);
            return Task.CompletedTask;
        }

        private void InitializeTheme()
        {
            // 初始化标题栏按钮背景色
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            // 注入标题栏（后台时无法获取标题栏）
            Container.RegisterInstance(titleBar);
            // 设置主题模式
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsKeys.THEME))
                return;
            var mode = (ThemeMode)ApplicationData.Current.LocalSettings.Values[SettingsKeys.THEME];
            if (mode == ThemeMode.System)
                return;
            var themeService = Container.Resolve<ThemeService>();
            themeService.SetThemeMode(mode);
        }

        protected override Type GetPageType(string pageToken)
        {
            var name = GetType().AssemblyQualifiedName;
            var format = name.Replace(GetType().FullName, $"{GetType().Namespace}.Views.{{0}}");
            var typeName = string.Format(CultureInfo.InvariantCulture, format, pageToken);
            var type = Type.GetType(typeName) ?? base.GetPageType(pageToken);
            return type;
        }

        #region ORIGINAL CODES

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        //protected override void OnLaunched(LaunchActivatedEventArgs e)
        //{
        //    Frame rootFrame = Window.Current.Content as Frame;

        //    // 不要在窗口已包含内容时重复应用程序初始化，
        //    // 只需确保窗口处于活动状态
        //    if (rootFrame == null)
        //    {
        //        // 创建要充当导航上下文的框架，并导航到第一页
        //        rootFrame = new Frame();

        //        rootFrame.NavigationFailed += OnNavigationFailed;

        //        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        //        {
        //            //TODO: 从之前挂起的应用程序加载状态
        //        }

        //        // 将框架放在当前窗口中
        //        Window.Current.Content = rootFrame;
        //    }

        //    if (e.PrelaunchActivated == false)
        //    {
        //        if (rootFrame.Content == null)
        //        {
        //            // 当导航堆栈尚未还原时，导航到第一页，
        //            // 并通过将所需信息作为导航参数传入来配置
        //            // 参数
        //            rootFrame.Navigate(typeof(MainPage), e.Arguments);
        //        }
        //        // 确保当前窗口处于活动状态
        //        Window.Current.Activate();
        //    }
        //}

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        //void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        //{
        //    throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        //}

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        //private void OnSuspending(object sender, SuspendingEventArgs e)
        //{
        //    var deferral = e.SuspendingOperation.GetDeferral();
        //    //TODO: 保存应用程序状态并停止任何后台活动
        //    deferral.Complete();
        //}
        #endregion
    }
}

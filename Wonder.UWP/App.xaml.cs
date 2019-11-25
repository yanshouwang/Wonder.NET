using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Practices.Unity;
using Prism.Unity.Windows;
using Prism.Windows.AppModel;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
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
            AppCenter.Start("4b6186ec-c43a-41f6-922f-d808d207ab4c", typeof(Analytics), typeof(Crashes));
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
            NavigationService.Navigate(ViewTokens.LE, null);
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Wonder.UWP.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Wonder.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel
            => DataContext as SettingsViewModel;

        public SettingsView()
        {
            this.InitializeComponent();

            UpArrowBTN.Click += UpArrowBTN_Click;
            DownArrowBTN.Click += DownArrowBTN_Click;
        }

        private void DownArrowBTN_Click(object sender, RoutedEventArgs e)
        {
            var verticalOffset = Math.Min(SettingsSCV.ScrollableHeight, SettingsSCV.VerticalOffset + SettingsSCV.ActualHeight / 2);
            SettingsSCV.ChangeView(null, verticalOffset, null);
        }

        private void UpArrowBTN_Click(object sender, RoutedEventArgs e)
        {
            var verticalOffset = Math.Max(0.0, SettingsSCV.VerticalOffset - SettingsSCV.ActualHeight / 2);
            SettingsSCV.ChangeView(null, verticalOffset, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

using MUXC = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Wonder.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LEView : Page
    {
        public LEViewModel ViewModel
            => DataContext as LEViewModel;

        public LEView()
        {
            this.InitializeComponent();
        }

        private void DevicesCBX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                DeviceView.Content = new LEDeviceView();
                LoggerView.Content = new LELoggerView();
            }
            else
            {
                var device = (LEDeviceViewModel)e.AddedItems[0];
                DeviceView.Content = new LEDeviceView() { DataContext = device };
                LoggerView.Content = new LELoggerView() { DataContext = device.LoggerX };
            }
        }
    }
}

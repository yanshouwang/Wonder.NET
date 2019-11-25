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
    public sealed partial class LEView : Page
    {
        public LEViewModel ViewModel
            => DataContext as LEViewModel;

        public LENode SelectedNode { get; set; }

        public LEView()
        {
            this.InitializeComponent();
        }

        private void ConnectBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedNode?.Item is LEDeviceViewModel device) ||
                !device.ConnectComamnd.CanExecute())
                return;
            if (ViewModel.StopScanCommand.CanExecute())
                ViewModel.StopScanCommand.Execute();
            device.ConnectComamnd.Execute();
        }

        private void DevicesTVW_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            SelectedNode = (LENode)args.InvokedItem;
        }
    }
}

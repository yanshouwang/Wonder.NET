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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Wonder.UWP.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Wonder.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BLEView : Page
    {
        private object _item;

        public BLEViewModel ViewModel
            => DataContext as BLEViewModel;

        public BLEView()
        {
            this.InitializeComponent();
        }

        private void OnDeviceClick(object sender, ItemClickEventArgs e)
        {
            _item = e.ClickedItem;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                var animation = DevicesLVW.PrepareConnectedAnimation("NEW", e.Parameter, "NameTBK");
                //animation.Configuration = new DirectConnectedAnimationConfiguration();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("BACK");
            if (animation != null && _item != null)
            {
                await DevicesLVW.TryStartConnectedAnimationAsync(animation, _item, "NameTBK");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(DevicesLVW.SelectedItem is BLEDeviceViewModel device))
                return;
            MessagesLVW.ItemsSource = device.Messages;
            try
            {
                await device.ConntecAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private void DisconnectBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!(DevicesLVW.SelectedItem is BLEDeviceViewModel device))
                return;
            MessagesLVW.ItemsSource = null;
            try
            {
                device.Disconnect();
            }
            catch (Exception ex)
            {

            }
        }
    }
}

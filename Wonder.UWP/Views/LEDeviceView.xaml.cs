﻿using System;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Wonder.UWP.Views
{
    public sealed partial class LEDeviceView : UserControl
    {
        public LEDeviceViewModel ViewModel
            => DataContext as LEDeviceViewModel;

        public LEDeviceView()
        {
            this.InitializeComponent();
        }

        private void ServicesCBX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                ServiceView.Content = new LEServiceView();
            }
            else
            {
                ServiceView.Content = new LEServiceView() { DataContext = e.AddedItems[0] };
            }
        }
    }
}
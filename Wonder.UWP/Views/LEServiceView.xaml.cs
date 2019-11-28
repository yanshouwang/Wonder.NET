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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace Wonder.UWP.Views
{
    public sealed partial class LEServiceView : UserControl
    {
        public LEServiceViewModel ViewModel
            => DataContext as LEServiceViewModel;

        public LEServiceView()
        {
            this.InitializeComponent();
        }

        private void CharacteristicsCBX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                CharacteristicView.Content = new LECharacteristicView();
            }
            else
            {
                CharacteristicView.Content = new LECharacteristicView() { DataContext = e.AddedItems[0] };
            }
        }
    }
}

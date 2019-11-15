using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Wonder.UWP.Extension;
using Wonder.UWP.Util;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Wonder.UWP.Views
{
    public sealed class HorizontalListView : ListView
    {
        public HorizontalListView()
        {
            this.DefaultStyleKey = typeof(HorizontalListView);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var viewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            viewer.Loaded += Viewer_Loaded;
        }

        private void Viewer_Loaded(object sender, RoutedEventArgs e)
        {
            var viewer = (ScrollViewer)sender;
            var decrease = viewer.GetChild("DecreaseButton") as RepeatButton;
            var increase = viewer.GetChild("IncreaseButton") as RepeatButton;
            decrease.Click += (s, args) =>
            {
                var horizontalOffset = Math.Max(0.0, viewer.HorizontalOffset - viewer.ExtentWidth / Items.Count);
                viewer.ChangeView(horizontalOffset, null, null);
            };
            increase.Click += (s, args) =>
            {
                var horizontalOffset = Math.Min(viewer.ScrollableWidth, viewer.HorizontalOffset + viewer.ExtentWidth / Items.Count);
                viewer.ChangeView(horizontalOffset, null, null);
            };
        }
    }
}

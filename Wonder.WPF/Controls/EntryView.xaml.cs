using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wonder.WPF.Controls
{
    /// <summary>
    /// CRCEntryView.xaml 的交互逻辑
    /// </summary>
    public partial class EntryView : UserControl
    {
        private string mPreviewText;

        // TODO: 属性不应该返回数组
        public byte[] Value
        {
            get { return (byte[])GetValue(ValueProperty); }
            private set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(byte[]), typeof(EntryView), new PropertyMetadata(null));

        public EntryMode Mode
        {
            get { return (EntryMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(EntryMode), typeof(EntryView), new PropertyMetadata(EntryMode.ASCII, OnEntryModePropertyChanged));

        private static void OnEntryModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (EntryView)d;
            view.UpdateEntryMode();
        }

        public EntryView()
        {
            InitializeComponent();
            UpdateEntryMode();
        }

        private void UpdateEntryMode()
        {
            switch (Mode)
            {
                case EntryMode.ASCII:
                    ASCII.IsChecked = true;
                    break;
                case EntryMode.HEX:
                    HEX.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (HEX.IsChecked != true || string.IsNullOrEmpty(TEXT.Text))
                return;
            if (TEXT.Text.Replace(" ", string.Empty).Length % 2 == 0)
            {
                TEXT.AppendText(" ");
                TEXT.CaretIndex = TEXT.Text.Length;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (HEX.IsChecked == true && !string.IsNullOrEmpty(TEXT.Text))
            {
                if (TEXT.Text.EndsWith(" ") || TEXT.Text.Replace(" ", string.Empty).Length % 2 != 0)
                {
                    TEXT.Text = TEXT.Text.Remove(TEXT.Text.Length - 1).TrimEnd();
                }
            }
            UpdateValue();
        }

        private void UpdateValue()
        {
            switch (Mode)
            {
                case EntryMode.ASCII:
                    Value = Encoding.ASCII.GetBytes(TEXT.Text);
                    break;
                case EntryMode.HEX:
                    var values = TEXT.Text.Split(" ");
                    Value = new byte[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        Value[i] = Convert.ToByte(values[i], 16);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnASCIIChecked(object sender, RoutedEventArgs e)
        {
            Mode = EntryMode.ASCII;
            SaveState();
        }

        private void OnHEXChecked(object sender, RoutedEventArgs e)
        {
            Mode = EntryMode.HEX;
            SaveState();
        }

        private void SaveState()
        {
            var text = TEXT.Text;
            if (string.IsNullOrEmpty(mPreviewText))
                TEXT.Clear();
            else
                TEXT.Text = mPreviewText;
            if (!string.IsNullOrEmpty(text))
                mPreviewText = text;
        }
    }
}

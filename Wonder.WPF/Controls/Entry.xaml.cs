﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wonder.WPF.Controls
{
    /// <summary>
    /// CRCEntryView.xaml 的交互逻辑
    /// </summary>
    public partial class Entry : UserControl
    {
        private string mPreviewText;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Entry), new PropertyMetadata(string.Empty, OnTextPropertyChanged, CorceTextValue));

        private static object CorceTextValue(DependencyObject d, object baseValue)
        {
            var view = (Entry)d;
            var value = (string)baseValue;
            // 如果处于 HEX 模式, 检查文本格式, 若文本格式不正确, 重置为当前文本
            if (view.Mode == EntryMode.HEX && !string.IsNullOrEmpty(value))
            {
                var regex = new Regex(@"^(?:[0-9A-Fa-f]{2}\s)*[0-9A-Fa-f]{2}$");
                if (!regex.IsMatch(value))
                {
                    value = view.Text;
                }
            }
            return value;
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (Entry)d;
            var value = (string)e.NewValue;
            if (view.TextView.Text == value)
                return;
            view.TextView.Text = value;
        }

        public EntryMode Mode
        {
            get { return (EntryMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(EntryMode), typeof(Entry), new PropertyMetadata(EntryMode.ASCII, OnEntryModePropertyChanged));

        private static void OnEntryModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (Entry)d;
            view.UpdateEntryMode();
        }

        public Entry()
        {
            InitializeComponent();

            mPreviewText = TextView.Text;

            UpdateEntryMode();
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            e.Handled = true;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Text = TextView.Text;
        }

        private void UpdateEntryMode()
        {
            switch (Mode)
            {
                case EntryMode.ASCII:
                    if (ASCIIView.IsChecked == true)
                        return;
                    ASCIIView.IsChecked = true;
                    break;
                case EntryMode.HEX:
                    if (HEXView.IsChecked == true)
                        return;
                    HEXView.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 如果处于 HEX 模式, 只允许输入十六进制字符
            if (HEXView.IsChecked != true)
                return;
            var regex = new Regex(@"^[0-9A-Fa-f]$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }
            if (string.IsNullOrEmpty(TextView.Text))
                return;
            if (TextView.Text.Replace(" ", string.Empty).Length % 2 == 0)
            {
                TextView.AppendText(" ");
                TextView.CaretIndex = TextView.Text.Length;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // 失去焦点时如果处于 HEX 模式, 需要校正文本结尾处
            if (HEXView.IsChecked != true || string.IsNullOrEmpty(TextView.Text))
                return;
            if (TextView.Text.EndsWith(" ") || TextView.Text.Replace(" ", string.Empty).Length % 2 != 0)
            {
                TextView.Text = TextView.Text.Remove(TextView.Text.Length - 1).TrimEnd();
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
            if (TextView.Text == mPreviewText)
                return;
            var text = TextView.Text;
            TextView.Text = mPreviewText;
            mPreviewText = text;
        }

        private void CanExecutePasteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text))
            {
                e.CanExecute = false;
            }
            else if (Mode != EntryMode.HEX)
            {
                e.CanExecute = true;
            }
            else
            {
                var regex = new Regex(@"^[0-9A-Fa-f\s]+$");
                e.CanExecute = regex.IsMatch(text);
            }
            e.Handled = true;
        }

        private void ExecutPasteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (Mode != EntryMode.HEX)
            {
                TextView.Text += Clipboard.GetText();
            }
            else
            {
                var str = TextView.Text + Clipboard.GetText();
                TextView.Text = str.Replace(" ", string.Empty).LoopInsert(2, " ");
            }
            // 光标移到末尾
            TextView.CaretIndex = TextView.Text.Length;
        }
    }
}

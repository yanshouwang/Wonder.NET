using System;
using Windows.UI.Xaml.Data;
using Wonder.UWP.ViewModels;

namespace Wonder.UWP.Converters
{
    class EndSymbolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var symbol = (EndSymbol)value;
            var str = symbol.ToDisplay();
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Wonder.Core.Security.Cryptography;

namespace Wonder.WPF.Converters
{
    class CRC2PolyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var crc = (CRC)value;
            var width = Math.Ceiling(crc.Width / 8.0) * 2;
            var poly = crc.Poly.ToString($"X{width}");
            return $"0x{poly}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

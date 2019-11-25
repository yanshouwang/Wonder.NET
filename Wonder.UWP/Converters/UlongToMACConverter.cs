using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Wonder.UWP.Converters
{
    public class UlongToMACConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var mac = string.Empty;
            if (value is ulong number)
            {
                var array = BitConverter.GetBytes(number).Take(6).Reverse().ToArray();
                mac = BitConverter.ToString(array).Replace("-", ":");
            }
            return mac;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

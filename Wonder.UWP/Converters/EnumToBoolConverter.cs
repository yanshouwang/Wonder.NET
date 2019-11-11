using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Wonder.UWP.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Target { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var source = (int)value;
            var target = (int)Target;
            var result = source == target;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Wonder.UWP.Converters
{
    public class ValueTypeToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var horizontalAlignment = HorizontalAlignment.Left;
            if (value is int type && type == 1)
            {
                horizontalAlignment = HorizontalAlignment.Right;
            }
            return horizontalAlignment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

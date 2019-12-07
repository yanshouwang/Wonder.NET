using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Wonder.UWP.ViewModels;

namespace Wonder.UWP.Converters
{
    public class LEWriteStateToIsWritingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (LEWriteState)value;
            var isWritting = state != LEWriteState.NotWriting;
            return isWritting;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

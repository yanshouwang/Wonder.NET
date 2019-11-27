using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Wonder.UWP.ViewModels;

namespace Wonder.UWP.Converters
{
    public class LEDeviceStateToBoolStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? state = null;
            if (value is LEDeviceState deviceState && (deviceState == LEDeviceState.Connected || deviceState == LEDeviceState.Disconnected))
                state = deviceState == LEDeviceState.Connected ? true : false;
            return state;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Wonder.UWP.Converters
{
    public class SelectionChangedEventArgsToSelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is SelectionChangedEventArgs args) ||
                args.AddedItems == null ||
                args.AddedItems.Count != 1)
                return null;

            var item = args.AddedItems[0];
            return item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

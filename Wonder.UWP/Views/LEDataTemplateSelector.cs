using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wonder.UWP.ViewModels;

namespace Wonder.UWP.Views
{
    public class LEDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Device { get; set; }
        public DataTemplate Service { get; set; }
        public DataTemplate Characteristic { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (!(item is LENode node))
                return base.SelectTemplateCore(item);
            else if (node.Item is LEDeviceViewModel)
                return Device;
            else if (node.Item is LEServiceViewModel)
                return Service;
            else if (node.Item is LECharacteristicViewModel)
                return Characteristic;
            else
                return base.SelectTemplateCore(item);
        }
    }
}

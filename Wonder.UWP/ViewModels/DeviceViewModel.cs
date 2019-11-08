using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace Wonder.UWP.ViewModels
{
    public class DeviceViewModel : BindableBase
    {
        public string ID { get; }
        public string Name { get; }

        public DeviceViewModel(string id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}

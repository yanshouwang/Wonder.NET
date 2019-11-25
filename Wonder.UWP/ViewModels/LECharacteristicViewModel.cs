using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Wonder.UWP.ViewModels
{
    public class LECharacteristicViewModel : BaseViewModel
    {
        private readonly GattCharacteristic _characteristic;

        public LECharacteristicViewModel(INavigationService navigationService, GattCharacteristic characteristic)
            : base(navigationService)
        {
            _characteristic = characteristic;
        }
    }
}

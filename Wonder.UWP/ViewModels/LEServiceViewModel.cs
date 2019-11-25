using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Wonder.UWP.ViewModels
{
    public class LEServiceViewModel : BaseViewModel
    {
        private readonly GattDeviceService _service;

        public IList<LECharacteristicViewModel> Characteristics { get; }

        public LEServiceViewModel(INavigationService navigationService, GattDeviceService service, IList<LECharacteristicViewModel> characteristics)
            : base(navigationService)
        {
            _service = service;
            Characteristics = characteristics;
        }
    }
}

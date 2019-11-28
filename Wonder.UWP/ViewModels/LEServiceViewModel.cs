using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LEServiceViewModel : BaseViewModel
    {
        private readonly GattDeviceService _service;

        public Guid UUID
            => _service.Uuid;
        public ObservableCollection<LECharacteristicViewModel> Characteristics { get; }
        public ILELoggerX LoggerX { get; }

        public LEServiceViewModel(INavigationService navigationService, GattDeviceService service, IList<LECharacteristicViewModel> characteristics, ILELoggerX loggerX)
            : base(navigationService)
        {
            _service = service;
            Characteristics = characteristics == null
                            ? new ObservableCollection<LECharacteristicViewModel>()
                            : new ObservableCollection<LECharacteristicViewModel>(characteristics);
            LoggerX = loggerX;
        }
    }
}

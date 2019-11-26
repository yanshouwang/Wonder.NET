using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Wonder.UWP.ViewModels
{
    public class LEDeviceViewModel : BaseViewModel
    {
        private BluetoothLEDevice _device;

        public ulong Address { get; set; }
        public string Name { get; set; }
        public ObservableCollection<LEServiceViewModel> Services { get; }

        private int _rssi;
        public int RSSI
        {
            get { return _rssi; }
            set { SetProperty(ref _rssi, value); }
        }

        private LEDeviceState _state;
        public LEDeviceState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        public LEDeviceViewModel(INavigationService navigationService, ulong address, string name, int rssi)
            : base(navigationService)
        {
            Address = address;
            Name = name;
            RSSI = rssi;
            Services = new ObservableCollection<LEServiceViewModel>();
        }

        private DelegateCommand _connectCommand;
        public DelegateCommand ConnectComamnd =>
            _connectCommand ?? (_connectCommand = new DelegateCommand(ExecuteConnectComamnd, CanExecuteConnectCommand).ObservesProperty(() => State));

        private bool CanExecuteConnectCommand()
            => State == LEDeviceState.Disconnected;

        async void ExecuteConnectComamnd()
        {
            State = LEDeviceState.Connecting;
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
            var sr = await _device.GetGattServicesAsync();
            if (sr.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in sr.Services)
                {
                    var cr = await service.GetCharacteristicsAsync();
                    if (cr.Status == GattCommunicationStatus.Success)
                    {
                        var items = cr.Characteristics.Select(i => new LECharacteristicViewModel(NavigationService, i)).ToList();
                        var item = new LEServiceViewModel(NavigationService, service, items);
                        Services.Add(item);
                    }
                    else
                    {
                        var item = new LEServiceViewModel(NavigationService, service);
                        Services.Add(item);
                    }
                }
            }
            State = _device.ConnectionStatus == BluetoothConnectionStatus.Connected
                  ? LEDeviceState.Connected
                  : LEDeviceState.Disconnected;
        }
    }
}

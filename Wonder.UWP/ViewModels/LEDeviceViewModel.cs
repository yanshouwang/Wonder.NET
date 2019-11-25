using System;
using System.Collections.Generic;
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
            _device.ConnectionStatusChanged += OnConnectionStatusChanged;
            var sr = await _device.GetGattServicesAsync();
            if (sr.Status != GattCommunicationStatus.Success)
            {
                State = LEDeviceState.Disconnected;
                return;
            }
            foreach (var service in sr.Services)
            {
                var cr = await service.GetCharacteristicsAsync();
                if (cr.Status != GattCommunicationStatus.Success)
                {
                    State = LEDeviceState.Disconnected;
                    return;
                }
            }
        }

        private async void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            await DispatcherRunAsync(() =>
            {
                State = sender.ConnectionStatus == BluetoothConnectionStatus.Connected
                      ? LEDeviceState.Connected
                      : LEDeviceState.Disconnected;
            });
        }
    }
}

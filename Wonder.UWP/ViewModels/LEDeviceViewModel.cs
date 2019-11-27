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

        private LEDeviceState _connectionState;
        public LEDeviceState ConnectionState
        {
            get { return _connectionState; }
            set { SetProperty(ref _connectionState, value); }
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
            _connectCommand ?? (_connectCommand = new DelegateCommand(ExecuteConnectComamnd, CanExecuteConnectCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteConnectCommand()
            => ConnectionState == LEDeviceState.Disconnected;

        async void ExecuteConnectComamnd()
        {
            ConnectionState = LEDeviceState.Connecting;
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
            _device.ConnectionStatusChanged += OnConnectionStatusChanged;
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
        }

        private async void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            await DispatcherRunAsync(() =>
            {
                switch (sender.ConnectionStatus)
                {
                    case BluetoothConnectionStatus.Disconnected:
                        {
                            ConnectionState = LEDeviceState.Disconnected;
                            Services.Clear();
                            break;
                        }
                    case BluetoothConnectionStatus.Connected:
                        {
                            ConnectionState = LEDeviceState.Connected;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            });
        }

        private DelegateCommand _disconnectCommand;
        public DelegateCommand DisconnectCommand =>
            _disconnectCommand ?? (_disconnectCommand = new DelegateCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteDisconnectCommand()
            => ConnectionState == LEDeviceState.Connected;

        void ExecuteDisconnectCommand()
        {
            ConnectionState = LEDeviceState.Disconnecting;
            _device.Dispose();
        }

        private DelegateCommand _switchConnectionStateCommand;
        public DelegateCommand SwitchConnectionStateCommand =>
            _switchConnectionStateCommand ?? (_switchConnectionStateCommand = new DelegateCommand(ExecuteSwitchConnectionStateCommand, CanExecuteSwitchConnectionStateCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteSwitchConnectionStateCommand()
            => ConnectionState != LEDeviceState.Connecting && ConnectionState != LEDeviceState.Disconnecting;

        void ExecuteSwitchConnectionStateCommand()
        {
            if (ConnectionState == LEDeviceState.Disconnected)
            {
                ExecuteConnectComamnd();
            }
            else
            {
                ExecuteDisconnectCommand();
            }
        }
    }
}

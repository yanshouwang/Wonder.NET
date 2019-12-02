using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LEDeviceViewModel : BaseViewModel, IDisposable
    {
        private BluetoothLEDevice _device;

        public ulong Address { get; set; }
        public string Name { get; set; }
        public ObservableCollection<LEServiceViewModel> Services { get; }
        public ILELoggerX LoggerX { get; }

        private int _rssi;
        public int RSSI
        {
            get { return _rssi; }
            set
            {
                SetProperty(ref _rssi, value);
                if (!IsLogRSSIEnabled)
                    return;
                LoggerX.LogRSSI(value);
            }
        }

        private LEDeviceState _connectionState;
        public LEDeviceState ConnectionState
        {
            get { return _connectionState; }
            set { SetProperty(ref _connectionState, value); }
        }

        private bool _isLogRSSIEnabled;
        public bool IsLogRSSIEnabled
        {
            get { return _isLogRSSIEnabled; }
            set { SetProperty(ref _isLogRSSIEnabled, value); }
        }

        public LEDeviceViewModel(INavigationService navigationService, ulong address, string name, int rssi)
            : base(navigationService)
        {
            Address = address;
            Name = name;
            RSSI = rssi;
            Services = new ObservableCollection<LEServiceViewModel>();
            var array = BitConverter.GetBytes(address).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", string.Empty);
            LoggerX = new FileLELoggerX(mac);
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
                try
                {
                    foreach (var service in sr.Services)
                    {
                        var cr = await service.GetCharacteristicsAsync();
                        if (cr.Status == GattCommunicationStatus.Success)
                        {
                            var items = cr.Characteristics.Select(i => new LECharacteristicViewModel(NavigationService, i, LoggerX)).ToList();
                            var item = new LEServiceViewModel(NavigationService, service, items, LoggerX);
                            Services.Add(item);
                        }
                        else
                        {
                            var item = new LEServiceViewModel(NavigationService, service, null, LoggerX);
                            Services.Add(item);
                        }
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Crashes.TrackError(ex);
                }
                ConnectionState = LEDeviceState.Connected;
            }
            else
            {
                ConnectionState = LEDeviceState.Disconnected;
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
                            sender.ConnectionStatusChanged -= OnConnectionStatusChanged;
                            ConnectionState = LEDeviceState.Disconnected;
                            if (_device != null)
                            {
                                ClearServices();
                                _device.Dispose();
                                _device = null;
                            }
                            break;
                        }
                    case BluetoothConnectionStatus.Connected:
                        {
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            });
        }

        private void ClearServices()
        {
            foreach (var service in Services)
            {
                service.Dispose();
            }
            Services.Clear();
        }

        private DelegateCommand _disconnectCommand;
        public DelegateCommand DisconnectCommand =>
            _disconnectCommand ?? (_disconnectCommand = new DelegateCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteDisconnectCommand()
            => ConnectionState == LEDeviceState.Connected;

        void ExecuteDisconnectCommand()
        {
            if (_device == null)
                return;
            ConnectionState = LEDeviceState.Disconnecting;
            ClearServices();
            _device.Dispose();
            _device = null;
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

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (_device != null)
                    {
                        ClearServices();
                        _device.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~LEDeviceViewModel()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        #region 事件
        public event EventHandler RSSIChanged;
        #endregion

        #region 字段
        private BluetoothLEDevice mDevice;
        #endregion

        #region 属性
        public ulong Address { get; set; }
        public string Name { get; set; }
        public ObservableCollection<LEServiceViewModel> Services { get; }

        private int mRSSI;
        public int RSSI
        {
            get { return mRSSI; }
            set
            {
                if (!SetProperty(ref mRSSI, value))
                    return;

                RSSIChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private LEDeviceState mConnectionState;
        public LEDeviceState ConnectionState
        {
            get { return mConnectionState; }
            set { SetProperty(ref mConnectionState, value); }
        }
        #endregion

        #region 构造
        public LEDeviceViewModel(INavigationService navigationService, ulong address, string name, int rssi)
            : base(navigationService)
        {
            Address = address;
            Name = name;
            RSSI = rssi;
            Services = new ObservableCollection<LEServiceViewModel>();
        }
        #endregion

        #region 方法

        private void ClearServices()
        {
            foreach (var service in Services)
            {
                service.Characteristics.Clear();
                service.Dispose();
            }
            Services.Clear();
        }
        #endregion

        #region 命令

        private DelegateCommand mConnectCommand;
        public DelegateCommand ConnectComamnd =>
            mConnectCommand ?? (mConnectCommand = new DelegateCommand(ExecuteConnectComamnd, CanExecuteConnectCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteConnectCommand()
            => ConnectionState == LEDeviceState.Disconnected;

        async void ExecuteConnectComamnd()
        {
            ConnectionState = LEDeviceState.Connecting;
            mDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
            mDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
            var sr = await mDevice.GetGattServicesAsync();
            if (sr.Status == GattCommunicationStatus.Success)
            {
                try
                {
                    foreach (var service in sr.Services)
                    {
                        var cr = await service.GetCharacteristicsAsync();
                        if (cr.Status == GattCommunicationStatus.Success)
                        {
                            var mtu = service.Session.MaxPduSize;
                            var items = cr.Characteristics.Select(i => new LECharacteristicViewModel(NavigationService, i, mtu)).ToList();
                            var item = new LEServiceViewModel(NavigationService, service, items);
                            Services.Add(item);
                        }
                        else
                        {
                            var item = new LEServiceViewModel(NavigationService, service, null);
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
                            if (mDevice != null)
                            {
                                ClearServices();
                                mDevice.Dispose();
                                mDevice = null;
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

        private DelegateCommand mDisconnectCommand;
        public DelegateCommand DisconnectCommand =>
            mDisconnectCommand ?? (mDisconnectCommand = new DelegateCommand(ExecuteDisconnectCommand, CanExecuteDisconnectCommand).ObservesProperty(() => ConnectionState));

        private bool CanExecuteDisconnectCommand()
            => ConnectionState == LEDeviceState.Connected;

        void ExecuteDisconnectCommand()
        {
            if (mDevice == null)
                return;
            ConnectionState = LEDeviceState.Disconnecting;
            ClearServices();
            mDevice.Dispose();
            mDevice = null;
        }

        private DelegateCommand mSwitchConnectionStateCommand;
        public DelegateCommand SwitchConnectionStateCommand =>
            mSwitchConnectionStateCommand ?? (mSwitchConnectionStateCommand = new DelegateCommand(ExecuteSwitchConnectionStateCommand, CanExecuteSwitchConnectionStateCommand).ObservesProperty(() => ConnectionState));

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
        #endregion

        #region IDisposable Support
        private bool mDisposed = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (mDevice != null)
                    {
                        ClearServices();
                        mDevice.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                mDisposed = true;
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

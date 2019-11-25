using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Wonder.UWP.Constants;

namespace Wonder.UWP.ViewModels
{
    public class BLEDeviceViewModel : BaseViewModel
    {
        #region 字段
        private readonly Guid _communicationServiceUUID;
        private readonly Guid _notifyCharacteristicUUID;
        private readonly Guid _writeCharacteristicUUID;
        private readonly ConcurrentQueue<byte[]> _values;
        private BluetoothLEDevice _device;
        private GattCharacteristic _notifyCharacteristic;
        private GattCharacteristic _writeCharacteristic;

        private CancellationTokenSource _dataSource;
        private TaskCompletionSource<bool> _connectSource;
        private TaskCompletionSource<object> _deviceSource;
        private TaskCompletionSource<bool> _liveSource;
        private SemaphoreSlim _connectSemaphore;
        private SemaphoreSlim _writeSemaphore;
        private readonly IDictionary<string, Action<string, string>> _handlers;
        #endregion

        #region 属性
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _mac;
        public string MAC
        {
            get { return _mac; }
            set { SetProperty(ref _mac, value); }
        }

        private int _rssi;
        public int RSSI
        {
            get { return _rssi; }
            set { SetProperty(ref _rssi, value); }
        }

        private BLEDeviceState _state;
        public BLEDeviceState State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }

        private ulong _address;
        public ulong Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public ObservableCollection<string> Messages { get; }
        #endregion

        public BLEDeviceViewModel(
            INavigationService navigationService,
            Guid communicationServiceUUID,
            Guid notifyCharacteristicUUID,
            Guid writeCharacteristicUUID,
            ulong address,
            string name,
            string mac,
            int rssi)
            : base(navigationService)
        {
            _communicationServiceUUID = communicationServiceUUID;
            _notifyCharacteristicUUID = notifyCharacteristicUUID;
            _writeCharacteristicUUID = writeCharacteristicUUID;
            _values = new ConcurrentQueue<byte[]>();
            _connectSemaphore = new SemaphoreSlim(1, 1);
            _writeSemaphore = new SemaphoreSlim(1, 1);
            _handlers = new Dictionary<string, Action<string, string>>(StringComparer.InvariantCulture)
            {
                [SCPICommands.CODE] = OnCodeReceived,
            };

            Address = address;
            Name = name;
            MAC = mac;
            RSSI = rssi;

            Messages = new ObservableCollection<string>();
        }

        private async void OnCodeReceived(string arg1, string arg2)
        {
            try
            {
                // 保证握手指令只发送一次
                await _connectSemaphore.WaitAsync();
                if (IsConnected)
                    return;

                await WriteAsync($"@Wonder");
                IsConnected = true;
                _connectSource?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _connectSource?.TrySetException(ex);
            }
            finally
            {
                _connectSemaphore?.Release();
            }
        }

        private DelegateCommand _goBackCommand;
        public DelegateCommand GoBackCommand =>
            _goBackCommand ?? (_goBackCommand = new DelegateCommand(ExecuteGoBackCommand));

        void ExecuteGoBackCommand()
        {
            if (!NavigationService.CanGoBack())
                return;
            NavigationService.GoBack();
        }

        public async Task ConntecAsync()
        {
            try
            {
                _connectSource = new TaskCompletionSource<bool>();
                using (var cts = new CancellationTokenSource())
                using (cts.Token.Register(() => OnConnectTimeout()))
                {
                    if (State != BLEDeviceState.Disconnected && State != BLEDeviceState.ConnectFailed)
                        return;
                    _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
                    _device.ConnectionStatusChanged += OnConnectionStatusChanged;
                    var csr = await _device.GetGattServicesForUuidAsync(_communicationServiceUUID);
                    if (csr.Status != GattCommunicationStatus.Success)
                        return;
                    var count = csr.Services.Count;
                    var service = csr.Services[0];
                    var ncr = await service.GetCharacteristicsForUuidAsync(_notifyCharacteristicUUID);
                    var wcr = await service.GetCharacteristicsForUuidAsync(_writeCharacteristicUUID);
                    if (ncr.Status != GattCommunicationStatus.Success || wcr.Status != GattCommunicationStatus.Success)
                        return;
                    _notifyCharacteristic = ncr.Characteristics[0];
                    _writeCharacteristic = wcr.Characteristics[0];
                    _notifyCharacteristic.ValueChanged += OnNotifyCharacteristicValueChanged;
                    var status = await _notifyCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    var task = Task.Run(() => HandleCharacteristicValues());
                    await _connectSource.Task;
                }
            }
            finally
            {
                _connectSource = null;
            }
        }

        private void HandleCharacteristicValues()
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                using (cts.Token.Register(() => ResetDevice()))
                {
                    _dataSource = cts;
                    var bufferList = new List<byte>();
                    var pattern0 = @"(?<MESSAGES>\S+\r\n)+(?<SEGMENT>\S*)";
                    var pattern1 = @"(?i)(?<CMD>CODE\?)|(?<CMD>\S+:ECHO\??),(?<RESULT>OK|ERROR),?(?<CONTENT>\S*)";
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            while (_values.TryDequeue(out byte[] buffer))
                            {
                                bufferList.AddRange(buffer);
                                var input0 = Encoding.UTF8.GetString(bufferList.ToArray());
                                var match0 = Regex.Match(input0, pattern0);
                                if (!match0.Success)
                                    continue;

                                bufferList.Clear();
                                Task.Run(async () => await DispatcherRunAsync(() => Messages.Add(input0)));

                                var captures = match0.Groups["MESSAGES"].Captures;
                                var segment = match0.Groups["SEGMENT"];

                                foreach (Capture capture in captures)
                                {
                                    var match1 = Regex.Match(capture.Value, pattern1);
                                    if (!match1.Success)
                                        continue;

                                    var cmd = match1.Groups["CMD"].Value;
                                    var result = match1.Groups["RESULT"].Value;
                                    var content = match1.Groups["CONTENT"].Value;

                                    if (!_handlers.ContainsKey(cmd))
                                        continue;

                                    Task.Run(() => _handlers[cmd].Invoke(result, content));
                                }

                                if (segment.Length > 0)
                                {
                                    bufferList.AddRange(Encoding.ASCII.GetBytes(segment.Value));
                                }
                            }

                            Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ResetDevice()
        {
            try
            {
                IsConnected = false;

                _notifyCharacteristic = null;
                _dataSource = null;
                while (!_values.IsEmpty)
                {
                    _values.TryDequeue(out var value);
                }
                _deviceSource?.TrySetException(new Exception("设备已经重置."));
                _liveSource?.TrySetException(new Exception("设备已经重置."));
            }
            catch (Exception ex)
            {

            }
        }

        private void OnConnectTimeout()
        {
            _connectSource.TrySetException(new TimeoutException());
            _device.Dispose();
        }

        public void Disconnect()
        {
            if (State != BLEDeviceState.Connected)
                return;
            _device.Dispose();
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {

        }

        public Task GetDeviceAsync()
        {
            throw new NotImplementedException();
        }

        private async Task WriteAsync(string str)
        {
            using (var writer = new DataWriter())
            {
                writer.WriteString(str);
                var value = writer.DetachBuffer();
                await _writeCharacteristic.WriteValueAsync(value);
            }
        }

        private void OnNotifyCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var value = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(value);
            _values.Enqueue(value);
        }
    }
}

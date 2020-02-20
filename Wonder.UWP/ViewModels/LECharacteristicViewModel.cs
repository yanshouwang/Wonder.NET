using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LECharacteristicViewModel : BaseViewModel
    {
        private readonly GattCharacteristic _characteristic;
        private readonly List<byte> _receivedSyncValues;

        private CancellationTokenSource _loopCTS;
        private CancellationTokenSource _syncCTS;
        private TaskCompletionSource<byte[]> _syncTCS;

        private bool _isNotifying;
        public bool IsNotifying
        {
            get { return _isNotifying; }
            set { SetProperty(ref _isNotifying, value); }
        }

        public Guid UUID
            => _characteristic.Uuid;
        public bool CanRead
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);
        public bool CanWrite
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);
        public bool CanWriteWithoutResponse
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);
        public bool CanNotify
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
        public bool CanIndicate
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate);
        public bool IsWritable
            => CanWrite || CanWriteWithoutResponse;
        public bool IsWriteOptionAvailable
            => CanWrite && CanWriteWithoutResponse && !IsWriting && LoopWriteState == LEWriteState.NotWriting && SyncWriteState == LEWriteState.NotWriting;
        public bool IsContinuousUploadable
            => IsNotifying && !IsWriting && LoopWriteState == LEWriteState.NotWriting && SyncWriteState == LEWriteState.NotWriting;

        public ILELoggerX LoggerX { get; }

        public ObservableCollection<EndSymbol> EndSymbols { get; }

        private EndSymbol _endSymbol;
        public EndSymbol EndSymbol
        {
            get { return _endSymbol; }
            set { SetProperty(ref _endSymbol, value); }
        }

        private bool _isWriting;
        public bool IsWriting
        {
            get { return _isWriting; }
            set { SetProperty(ref _isWriting, value); }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private bool _isWriteWithoutResponse;
        public bool IsWriteWithoutResponse
        {
            get { return _isWriteWithoutResponse; }
            set { SetProperty(ref _isWriteWithoutResponse, value); }
        }

        private bool _isLoopWrite;
        public bool IsLoopWrite
        {
            get { return _isLoopWrite; }
            set { SetProperty(ref _isLoopWrite, value); }
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set { SetProperty(ref _interval, value); }
        }

        private LEWriteState _loopWriteState;
        public LEWriteState LoopWriteState
        {
            get { return _loopWriteState; }
            set { SetProperty(ref _loopWriteState, value); }
        }

        private LEWriteState _syncWriteState;
        public LEWriteState SyncWriteState
        {
            get { return _syncWriteState; }
            set { SetProperty(ref _syncWriteState, value); }
        }

        private bool _isContinuousUpload;
        public bool IsContinuousUpload
        {
            get { return _isContinuousUpload; }
            set { SetProperty(ref _isContinuousUpload, value); }
        }

        private bool _isContinuousUploading;
        public bool IsContinuousUploading
        {
            get { return _isContinuousUploading; }
            set { SetProperty(ref _isContinuousUploading, value); }
        }

        public LECharacteristicViewModel(INavigationService navigationService, GattCharacteristic characteristic, ILELoggerX loggerX)
            : base(navigationService)
        {
            _characteristic = characteristic;
            _receivedSyncValues = new List<byte>();
            LoggerX = loggerX;
            IsWriteWithoutResponse = CanWriteWithoutResponse && !CanWrite;
            var symbols = Enum.GetValues(typeof(EndSymbol)).Cast<EndSymbol>();
            EndSymbols = new ObservableCollection<EndSymbol>(symbols);
        }

        private DelegateCommand _startNotificationCommand;
        public DelegateCommand StartNotificationCommand =>
            _startNotificationCommand ?? (_startNotificationCommand = new DelegateCommand(ExecuteStartNotificationCommand, CanExecuteStartNotificationCommand).ObservesProperty(() => CanNotify).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStartNotificationCommand()
            => CanNotify && !IsNotifying;

        async void ExecuteStartNotificationCommand()
        {
            var status = await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (status != GattCommunicationStatus.Success)
                return;
            _characteristic.ValueChanged += OnValueChanged;
            IsNotifying = true;
            RaisePropertyChanged(nameof(IsContinuousUploadable));
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var value);
            if (IsContinuousUploadable)
            {
                if (!IsContinuousUploading)
                {
                    IsContinuousUploading = true;
                    LoggerX.LogContinuousUploadStarted();
                }
                LoggerX.LogContinuousUpload(value);
            }
            else if (IsContinuousUploading)
            {
                LoggerX.LogContinuousUpload(value);
                IsContinuousUploading = false;
                LoggerX.LogContinuousUploadStopped();
            }
            else if (SyncWriteState == LEWriteState.NotWriting)
            {
                LoggerX.LogValueChanged(value);
            }
            else
            {
                _receivedSyncValues.AddRange(value);
                var i = 0;
                while (i < _receivedSyncValues.Count - 1)
                {
                    var j = i + 1;
                    if (_receivedSyncValues[i] == 0x0D &&
                        _receivedSyncValues[j] == 0x0A)
                        break;
                    i++;
                }
                var count = i + 2;
                if (count > _receivedSyncValues.Count)
                    return;
                var received = _receivedSyncValues.Take(count).ToArray();
                _receivedSyncValues.RemoveRange(0, count);
                _syncTCS.SetResult(received);
            }
        }

        private DelegateCommand _stopNotificationCommand;
        public DelegateCommand StopNotificationCommand =>
            _stopNotificationCommand ?? (_stopNotificationCommand = new DelegateCommand(ExecuteStopNotificationCommand, CanExecuteStopNotificationCommand).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStopNotificationCommand()
            => IsNotifying;

        async void ExecuteStopNotificationCommand()
        {
            var status = await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            if (status != GattCommunicationStatus.Success)
                return;
            _characteristic.ValueChanged -= OnValueChanged;
            IsNotifying = false;
            RaisePropertyChanged(nameof(IsContinuousUploadable));
        }

        private DelegateCommand _writeCommand;
        public DelegateCommand WriteCommand =>
            _writeCommand ?? (_writeCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsContinuousUploading));

        private bool CanExecuteWriteCommand()
            => IsWritable && !string.IsNullOrWhiteSpace(Value) && !IsWriting && LoopWriteState == LEWriteState.NotWriting && SyncWriteState == LEWriteState.NotWriting && !IsContinuousUploading;

        async void ExecuteWriteCommand()
        {
            IsWriting = true;
            var value = EndSymbol == EndSymbol.None
                      ? Encoding.UTF8.GetBytes($"{Value}")
                      : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
            var result = await WriteAsync(value);
            LoggerX.LogWrite(value, result);
            IsWriting = false;
        }

        private async Task<bool> WriteAsync(byte[] data)
        {
            if (!CanWrite && !CanWriteWithoutResponse)
                return false;
            var option = IsWriteWithoutResponse ? GattWriteOption.WriteWithoutResponse : GattWriteOption.WriteWithResponse;
            // 大于 20 字节分包发送（最大可以支持 244 字节）
            // https://stackoverflow.com/questions/53313117/cannot-write-large-byte-array-to-a-ble-device-using-uwp-apis-e-g-write-value
            var count = data.Length / 20;
            var remainder = data.Length % 20;
            var carriage = new byte[20];
            for (int i = 0; i < count; i++)
            {
                Array.Copy(data, i * 20, carriage, 0, 20);
                var value = CryptographicBuffer.CreateFromByteArray(carriage);
                var status = await _characteristic.WriteValueAsync(value, option);
                //var result = await _characteristic.WriteValueWithResultAsync(value, option);
                //var status = result.Status;
                if (status != GattCommunicationStatus.Success)
                    return false;
            }
            if (remainder > 0)
            {
                carriage = new byte[remainder];
                Array.Copy(data, count * 20, carriage, 0, remainder);
                var value = CryptographicBuffer.CreateFromByteArray(carriage);
                var status = await _characteristic.WriteValueAsync(value, option);
                var isWritten = status == GattCommunicationStatus.Success;
                return isWritten;
            }
            return true;
        }

        private DelegateCommand _startLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            _startLoopWriteCommand ?? (_startLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsContinuousUploading).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => Interval));

        private bool CanExecuteStartLoopWriteCommand()
            => WriteCommand.CanExecute() && IsLoopWrite && Interval >= 0;

        async void ExecuteStartLoopWriteCommand()
        {
            LoopWriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            RaisePropertyChanged(nameof(IsContinuousUploadable));
            LoggerX.LogLoopWriteStarted();
            using (var loopCTS = new CancellationTokenSource())
            {
                _loopCTS = loopCTS;
                while (!loopCTS.Token.IsCancellationRequested)
                {
                    var value = Encoding.UTF8.GetBytes(Value);
                    var result = await WriteAsync(value);
                    LoggerX.LogLoopWrite(value, result);
                    await Task.Delay(Interval);
                }
                LoggerX.LogLoopWriteStopped();
                LoopWriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
                RaisePropertyChanged(nameof(IsContinuousUploadable));
            }
        }

        private DelegateCommand _stopLoopWriteCommand;
        public DelegateCommand StopLoopWriteCommand =>
            _stopLoopWriteCommand ?? (_stopLoopWriteCommand = new DelegateCommand(ExecuteStopLoopWriteCommand, CanExecuteStopLoopWriteCommand).ObservesProperty(() => LoopWriteState));

        private bool CanExecuteStopLoopWriteCommand()
            => LoopWriteState == LEWriteState.Writing;

        void ExecuteStopLoopWriteCommand()
        {
            LoopWriteState = LEWriteState.StopWriting;
            _loopCTS.Cancel();
            _loopCTS = null;
        }

        private DelegateCommand _switchLoopWriteCommand;
        public DelegateCommand SwitchLoopWriteCommand =>
            _switchLoopWriteCommand ?? (_switchLoopWriteCommand = new DelegateCommand(ExecuteSwitchLoopWriteCommand, CanExecuteSwitchLoopWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsContinuousUploading).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => Interval));

        private bool CanExecuteSwitchLoopWriteCommand()
            => StartLoopWriteCommand.CanExecute() || StopLoopWriteCommand.CanExecute();

        void ExecuteSwitchLoopWriteCommand()
        {
            if (StartLoopWriteCommand.CanExecute())
            {
                StartLoopWriteCommand.Execute();
            }
            else
            {
                StopLoopWriteCommand.Execute();
            }
        }

        private DelegateCommand _startSyncWriteCommand;
        public DelegateCommand StartSyncWriteCommand =>
            _startSyncWriteCommand ?? (_startSyncWriteCommand = new DelegateCommand(ExecuteStartSyncWriteCommand, CanExecuteStartSyncWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsContinuousUploading));

        private bool CanExecuteStartSyncWriteCommand()
            => WriteCommand.CanExecute();

        async void ExecuteStartSyncWriteCommand()
        {
            SyncWriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            RaisePropertyChanged(nameof(IsContinuousUploadable));
            LoggerX.LogSyncWriteStarted();
            using (var syncCTS = new CancellationTokenSource())
            {
                _syncCTS = syncCTS;
                while (!syncCTS.Token.IsCancellationRequested)
                {
                    var send = EndSymbol == EndSymbol.None
                             ? Encoding.UTF8.GetBytes($"{Value}")
                             : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
                    var received = await SyncWriteAsync(send);
                    LoggerX.LogSyncWrite(send, received);
                }
                LoggerX.LogSyncWriteStopped();
                SyncWriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
                RaisePropertyChanged(nameof(IsContinuousUploadable));
            }
        }

        private async Task<byte[]> SyncWriteAsync(byte[] array)
        {
            _syncTCS = new TaskCompletionSource<byte[]>();
            if (!await WriteAsync(array))
            {
                _syncTCS.SetException(new IOException("写入失败"));
            }
            var value = await _syncTCS.Task;
            return value;
        }

        private DelegateCommand _stopSyncWriteCommand;
        public DelegateCommand StopSyncWriteCommand =>
            _stopSyncWriteCommand ?? (_stopSyncWriteCommand = new DelegateCommand(ExecuteStopSyncWriteCommand, CanExecuteStopSyncWriteCommand).ObservesProperty(() => SyncWriteState));

        private bool CanExecuteStopSyncWriteCommand()
            => SyncWriteState == LEWriteState.Writing;

        void ExecuteStopSyncWriteCommand()
        {
            SyncWriteState = LEWriteState.StopWriting;
            _syncCTS.Cancel();
            _syncCTS = null;
        }
    }
}

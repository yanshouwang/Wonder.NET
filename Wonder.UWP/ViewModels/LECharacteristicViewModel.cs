using System;
using System.Collections.Generic;
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

        private CancellationTokenSource _loopSource;

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
            => CanWrite && CanWriteWithoutResponse;

        public ILELoggerX LoggerX { get; }

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

        private bool _isLoopWriting;
        public bool IsLoopWriting
        {
            get { return _isLoopWriting; }
            set { SetProperty(ref _isLoopWriting, value); }
        }

        public LECharacteristicViewModel(INavigationService navigationService, GattCharacteristic characteristic, ILELoggerX loggerX)
            : base(navigationService)
        {
            _characteristic = characteristic;
            LoggerX = loggerX;
            IsWriteWithoutResponse = CanWriteWithoutResponse && !CanWrite;
        }

        private async Task<bool> WriteAsync(byte[] array)
        {
            if (!CanWrite && !CanWriteWithoutResponse)
                return false;
            var value = CryptographicBuffer.CreateFromByteArray(array);
            var option = IsWriteWithoutResponse
                       ? GattWriteOption.WriteWithoutResponse
                       : GattWriteOption.WriteWithResponse;
            var status = await _characteristic.WriteValueAsync(value, option);
            //var result = await _characteristic.WriteValueWithResultAsync(value);
            //var status = result.Status;
            var isWritten = status == GattCommunicationStatus.Success;
            return isWritten;
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var value);
            LoggerX.LogValueChanged(value);
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
        }

        private DelegateCommand _writeCommand;
        public DelegateCommand WriteCommand =>
            _writeCommand ?? (_writeCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value));

        private bool CanExecuteWriteCommand()
            => IsWritable && !string.IsNullOrWhiteSpace(Value);

        async void ExecuteWriteCommand()
        {
            var value = Encoding.UTF8.GetBytes(Value);
            var result = await WriteAsync(value);
            LoggerX.LogWrite(value, result);
        }

        private DelegateCommand _startLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            _startLoopWriteCommand ?? (_startLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => IsLoopWriting).ObservesProperty(() => Interval));

        private bool CanExecuteStartLoopWriteCommand()
            => WriteCommand.CanExecute() && IsLoopWrite && !IsLoopWriting && Interval >= 0;

        async void ExecuteStartLoopWriteCommand()
        {
            IsLoopWriting = true;
            LoggerX.LogLoopWriteStarted();
            using (var loopSource = new CancellationTokenSource())
            {
                _loopSource = loopSource;
                while (!loopSource.Token.IsCancellationRequested)
                {
                    var value = Encoding.UTF8.GetBytes(Value);
                    var result = await WriteAsync(value);
                    LoggerX.LogLoopWrite(value, result);
                    await Task.Delay(Interval);
                }
                LoggerX.LogLoopWriteStopped();
            }
        }

        private DelegateCommand _stopLoopWriteCommand;
        public DelegateCommand StopLoopWriteCommand =>
            _stopLoopWriteCommand ?? (_stopLoopWriteCommand = new DelegateCommand(ExecuteStopLoopWriteCommand, CanExecuteStopLoopWriteCommand).ObservesProperty(() => IsLoopWriting));

        private bool CanExecuteStopLoopWriteCommand()
            => IsLoopWriting;

        void ExecuteStopLoopWriteCommand()
        {
            IsLoopWriting = false;
            _loopSource.Cancel();
            _loopSource = null;
        }

        private DelegateCommand _switchLoopWriteCommand;
        public DelegateCommand SwitchLoopWriteCommand =>
            _switchLoopWriteCommand ?? (_switchLoopWriteCommand = new DelegateCommand(ExecuteSwitchLoopWriteCommand, CanExecuteSwitchLoopWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => Interval));

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
    }
}

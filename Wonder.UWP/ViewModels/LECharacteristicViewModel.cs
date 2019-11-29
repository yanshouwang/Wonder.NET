using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly DispatcherTimer _timer;

        private string _stressStr = "ABCDEFGHIJKLMNOPQRST";

        private bool _isNotifying;
        public bool IsNotifying
        {
            get { return _isNotifying; }
            set { SetProperty(ref _isNotifying, value); }
        }

        public Guid UUID
            => _characteristic.Uuid;
        public bool CanWrite
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write) ||
               _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);
        public bool CanNotify
            => _characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);

        public ILELoggerX LoggerX { get; }

        private bool _isLoopWriteEnabled;
        public bool IsLoopWriteEnabled
        {
            get { return _isLoopWriteEnabled; }
            set { SetProperty(ref _isLoopWriteEnabled, value); }
        }

        private bool _isLoopWriting;
        public bool IsLoopWriting
        {
            get { return _isLoopWriting; }
            set { SetProperty(ref _isLoopWriting, value); }
        }

        private int _interval = 1000;
        public int Interval
        {
            get { return _interval; }
            set { SetProperty(ref _interval, value); }
        }

        private bool _isStressWriting;
        public bool IsStressWriting
        {
            get { return _isStressWriting; }
            set { SetProperty(ref _isStressWriting, value); }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public LECharacteristicViewModel(INavigationService navigationService, GattCharacteristic characteristic, ILELoggerX loggerX)
            : base(navigationService)
        {
            _characteristic = characteristic;
            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerTick;
            LoggerX = loggerX;
        }

        private async void OnTimerTick(object sender, object e)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(Value, BinaryStringEncoding.Utf8);
            GattCommunicationStatus status;
            if (_characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
            {
                status = await _characteristic.WriteValueAsync(buffer);
            }
            else
            {
                var result = await _characteristic.WriteValueWithResultAsync(buffer);
                status = result.Status;
            }
            var array = Encoding.UTF8.GetBytes(Value);
            var result1 = status == GattCommunicationStatus.Success ? true : false;
            LoggerX.LogSend(array, result1);
        }

        private async void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var value);
            if (LoggerX.IsStressWriting)
            {
                // 校验
                var str = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, args.CharacteristicValue);
                var result = str == _stressStr;
                LoggerX.LogReceived(value, result);
                if (IsStressWriting)
                {
                    _stressStr = _stressStr.Length == 20 ? "ABCDEFGHIJKLMNOPQRS" : "ABCDEFGHIJKLMNOPQRST";
                    var buffer = CryptographicBuffer.ConvertStringToBinary(_stressStr, BinaryStringEncoding.Utf8);
                    GattCommunicationStatus status;
                    if (_characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
                    {
                        status = await _characteristic.WriteValueAsync(buffer);
                    }
                    else
                    {
                        var result1 = await _characteristic.WriteValueWithResultAsync(buffer);
                        status = result1.Status;
                    }
                    var array = Encoding.UTF8.GetBytes(_stressStr);
                    var result2 = status == GattCommunicationStatus.Success ? true : false;
                    LoggerX.LogSend(array, result2);
                }
                else
                {
                    LoggerX.LogStressWriteStopped();
                }
            }
            else
            {
                LoggerX.LogReceived(value);
            }
        }

        private DelegateCommand _startNotificationCommand;
        public DelegateCommand StartNotificationCommand =>
            _startNotificationCommand ?? (_startNotificationCommand = new DelegateCommand(ExecuteStartNotificationCommand, CanExecuteStartNotificationCommand).ObservesProperty(() => IsNotifying));

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
            _stopNotificationCommand ?? (_stopNotificationCommand = new DelegateCommand(ExecuteStopNotificationCommand, CanExecuteStopNotificationCommand).ObservesProperty(() => IsNotifying).ObservesProperty(() => IsStressWriting));

        private bool CanExecuteStopNotificationCommand()
            => CanNotify && IsNotifying && !IsStressWriting;

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
            _writeCommand ?? (_writeCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => CanWrite).ObservesProperty(() => IsLoopWriting).ObservesProperty(() => IsStressWriting));

        private bool CanExecuteWriteCommand()
            => CanWrite && !IsStressWriting;

        async void ExecuteWriteCommand()
        {
            var value = Encoding.UTF8.GetBytes(Value);
            var result = await WriteAsync(value);
            LoggerX.LogSend(value, result);
        }

        private DelegateCommand _startLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            _startLoopWriteCommand ?? (_startLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => CanWrite).ObservesProperty(() => IsLoopWriteEnabled).ObservesProperty(() => IsLoopWriting).ObservesProperty(() => Interval).ObservesProperty(() => IsStressWriting));

        private bool CanExecuteStartLoopWriteCommand()
            => CanWrite && IsLoopWriteEnabled && !IsLoopWriting && Interval > 0 && !IsStressWriting;

        async void ExecuteStartLoopWriteCommand()
        {
            IsLoopWriting = true;
            var value = Encoding.UTF8.GetBytes(Value);
            var result = await WriteAsync(value);
            LoggerX.LogSend(value, result);
            _timer.Interval = TimeSpan.FromMilliseconds(Interval);
            _timer.Start();
        }

        private DelegateCommand _stopLoopWriteCommand;
        public DelegateCommand StopLoopWriteCommand =>
            _stopLoopWriteCommand ?? (_stopLoopWriteCommand = new DelegateCommand(ExecuteStopLoopWriteCommand, CanExecuteStopLoopWriteCommand).ObservesProperty(() => IsLoopWriting));

        private bool CanExecuteStopLoopWriteCommand()
            => IsLoopWriting;

        void ExecuteStopLoopWriteCommand()
        {
            _timer.Stop();
            IsLoopWriting = false;
        }

        private DelegateCommand _switchLoopWriteCommand;
        public DelegateCommand SwitchLoopWriteCommand =>
            _switchLoopWriteCommand ?? (_switchLoopWriteCommand = new DelegateCommand(ExecuteSwitchLoopWriteCommand, CanExecuteSwitchLoopWriteCommand).ObservesProperty(() => CanWrite).ObservesProperty(() => IsLoopWriteEnabled).ObservesProperty(() => Interval).ObservesProperty(() => IsStressWriting));

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

        private DelegateCommand _startStressWriteCommand;
        public DelegateCommand StartStressWriteCommand =>
            _startStressWriteCommand ?? (_startStressWriteCommand = new DelegateCommand(ExecuteStartStressWriteCommand, CanExecuteStartStressWriteCommand).ObservesProperty(() => CanWrite).ObservesProperty(() => IsStressWriting).ObservesProperty(() => IsLoopWriting).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStartStressWriteCommand()
            => CanWrite && !IsStressWriting && !IsLoopWriting && IsNotifying;

        async void ExecuteStartStressWriteCommand()
        {
            IsStressWriting = true;
            LoggerX.LogStressWriteStarted();
            var value = Encoding.UTF8.GetBytes(_stressStr);
            var result = await WriteAsync(value);
            LoggerX.LogSend(value, result);
        }

        private async Task<bool> WriteAsync(byte[] array)
        {
            var value = CryptographicBuffer.CreateFromByteArray(array);
            if (_characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
            {
                var status = await _characteristic.WriteValueAsync(value);
                var isWritten = status == GattCommunicationStatus.Success;
                return isWritten;
            }
            else
            {
                var result = await _characteristic.WriteValueWithResultAsync(value);
                var status = result.Status;
                var isWritten = status == GattCommunicationStatus.Success;
                return isWritten;
            }
        }

        private DelegateCommand _stopStressWriteCommand;
        public DelegateCommand StopStressWriteCommand =>
            _stopStressWriteCommand ?? (_stopStressWriteCommand = new DelegateCommand(ExecuteStopStressWriteCommand, CanExecuteStopStressWriteCommand).ObservesProperty(() => IsStressWriting));

        private bool CanExecuteStopStressWriteCommand()
            => IsStressWriting;

        void ExecuteStopStressWriteCommand()
        {
            IsStressWriting = false;
        }
    }
}

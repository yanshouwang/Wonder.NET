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

        private void OnTimerTick(object sender, object e)
        {
            WriteCommand.Execute();
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var value);
            LoggerX.LogReceived(value);
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
            _stopNotificationCommand ?? (_stopNotificationCommand = new DelegateCommand(ExecuteStopNotificationCommand, CanExecuteStopNotificationCommand).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStopNotificationCommand()
            => CanNotify && IsNotifying;

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
            _writeCommand ?? (_writeCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => CanWrite));

        private bool CanExecuteWriteCommand()
            => CanWrite;

        async void ExecuteWriteCommand()
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
            var isSuccess = status == GattCommunicationStatus.Success ? true : false;
            LoggerX.LogSend(array, isSuccess);
        }

        private DelegateCommand _startLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            _startLoopWriteCommand ?? (_startLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => CanWrite).ObservesProperty(() => IsLoopWriteEnabled).ObservesProperty(() => IsLoopWriting).ObservesProperty(() => Interval));

        private bool CanExecuteStartLoopWriteCommand()
            => CanWrite && IsLoopWriteEnabled && !IsLoopWriting && Interval > 0;

        void ExecuteStartLoopWriteCommand()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(Interval);
            _timer.Start();
            IsLoopWriting = true;
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
    }
}

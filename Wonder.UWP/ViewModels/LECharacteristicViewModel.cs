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

namespace Wonder.UWP.ViewModels
{
    public class LECharacteristicViewModel : BaseViewModel
    {
        #region 事件
        public event EventHandler<LECharacteristicValueEventArgs> ValueChanged;
        #endregion

        #region 字段
        private readonly GattCharacteristic mCharacteristic;
        #endregion

        #region 属性
        public ObservableCollection<EndSymbol> EndSymbols { get; }

        private int mMTU;
        public int MTU
        {
            get { return mMTU; }
            set { SetProperty(ref mMTU, value); }
        }

        private int mCTU;
        public int CTU
        {
            get { return mCTU; }
            set { SetProperty(ref mCTU, value); }
        }

        private EndSymbol mEndSymbol;
        public EndSymbol EndSymbol
        {
            get { return mEndSymbol; }
            set { SetProperty(ref mEndSymbol, value); }
        }

        private bool mIsNotifying;
        public bool IsNotifying
        {
            get { return mIsNotifying; }
            set { SetProperty(ref mIsNotifying, value); }
        }

        public Guid UUID
            => mCharacteristic.Uuid;
        public bool CanRead
            => mCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read);
        public bool CanWrite
            => mCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);
        public bool CanWriteWithoutResponse
            => mCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);
        public bool CanNotify
            => mCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify);
        public bool CanIndicate
            => mCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate);
        public bool IsWritable
            => CanWrite || CanWriteWithoutResponse;
        public bool IsWriteOptionAvailable
            => CanWrite && CanWriteWithoutResponse && !IsWriting;

        private bool mIsWriting;
        public bool IsWriting
        {
            get { return mIsWriting; }
            set { SetProperty(ref mIsWriting, value); }
        }

        private string mValue;
        public string Value
        {
            get { return mValue; }
            set { SetProperty(ref mValue, value); }
        }

        private bool mIsWriteWithoutResponse;
        public bool IsWriteWithoutResponse
        {
            get { return mIsWriteWithoutResponse; }
            set { SetProperty(ref mIsWriteWithoutResponse, value); }
        }
        #endregion

        #region 构造
        public LECharacteristicViewModel(INavigationService navigationService, GattCharacteristic characteristic, int mtu)
            : base(navigationService)
        {
            mCharacteristic = characteristic;
            MTU = mtu;
            // 默认 20 字节分包
            CTU = 20;
            IsWriteWithoutResponse = CanWriteWithoutResponse && !CanWrite;

            var symbols = Enum.GetValues(typeof(EndSymbol)).Cast<EndSymbol>();
            EndSymbols = new ObservableCollection<EndSymbol>(symbols);
        }
        #endregion

        #region 方法
        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var e = new LECharacteristicValueEventArgs(args.CharacteristicValue);
            ValueChanged?.Invoke(this, e);
        }

        public async Task StartNotifyAsync()
        {
            var status = await mCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            IsNotifying = status == GattCommunicationStatus.Success;
            if (!IsNotifying)
                return;

            mCharacteristic.ValueChanged += OnValueChanged;
        }

        public async Task StopNotifyAsync()
        {
            var status = await mCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            IsNotifying = status != GattCommunicationStatus.Success;
            if (IsNotifying)
                return;

            mCharacteristic.ValueChanged -= OnValueChanged;
        }

        public async Task<bool> WriteAsync(byte[] data)
        {
            if (!CanWrite && !CanWriteWithoutResponse)
                return false;
            var option = IsWriteWithoutResponse ? GattWriteOption.WriteWithoutResponse : GattWriteOption.WriteWithResponse;
            //var value = CryptographicBuffer.CreateFromByteArray(data);
            //var status = await mCharacteristic.WriteValueAsync(value, option);
            //return status == GattCommunicationStatus.Success;

            // 大于 20 字节分包发送（最大可以支持 512 字节）
            // https://stackoverflow.com/questions/53313117/cannot-write-large-byte-array-to-a-ble-device-using-uwp-apis-e-g-write-value
            var count = data.Length / CTU;
            var remainder = data.Length % CTU;
            var carriage = new byte[CTU];
            for (int i = 0; i < count; i++)
            {
                Array.Copy(data, i * CTU, carriage, 0, CTU);
                var value = CryptographicBuffer.CreateFromByteArray(carriage);
                var status = await mCharacteristic.WriteValueAsync(value, option);
                //var result = await mCharacteristic.WriteValueWithResultAsync(value, option);
                //var status = result.Status;
                if (status != GattCommunicationStatus.Success)
                    return false;
            }
            if (remainder > 0)
            {
                carriage = new byte[remainder];
                Array.Copy(data, count * CTU, carriage, 0, remainder);
                var value = CryptographicBuffer.CreateFromByteArray(carriage);
                var status = await mCharacteristic.WriteValueAsync(value, option);
                var isWritten = status == GattCommunicationStatus.Success;
                return isWritten;
            }
            return true;
        }
        #endregion

        #region 命令
        private DelegateCommand mStartNotifyCommand;
        public DelegateCommand StartNotifyCommand =>
            mStartNotifyCommand ?? (mStartNotifyCommand = new DelegateCommand(ExecuteStartNotifyCommand, CanExecuteStartNotifyCommand).ObservesProperty(() => CanNotify).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStartNotifyCommand()
            => CanNotify && !IsNotifying;

        async void ExecuteStartNotifyCommand()
        {
            await StartNotifyAsync();
        }

        private DelegateCommand mStopNotifyCommand;
        public DelegateCommand StopNotifyCommand =>
            mStopNotifyCommand ?? (mStopNotifyCommand = new DelegateCommand(ExecuteStopNotifyCommand, CanExecuteStopNotifyCommand).ObservesProperty(() => IsNotifying));

        private bool CanExecuteStopNotifyCommand()
            => IsNotifying;

        async void ExecuteStopNotifyCommand()
        {
            await StopNotifyAsync();
        }

        private DelegateCommand mWriteCommand;
        public DelegateCommand WriteCommand =>
            mWriteCommand ?? (mWriteCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting));

        private bool CanExecuteWriteCommand()
            => IsWritable && !string.IsNullOrWhiteSpace(Value) && !IsWriting;

        async void ExecuteWriteCommand()
        {
            IsWriting = true;
            var value = EndSymbol == EndSymbol.None
                      ? Encoding.UTF8.GetBytes($"{Value}")
                      : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
            await WriteAsync(value);
            IsWriting = false;
        }
        #endregion
    }
}

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
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Security.Cryptography;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LEViewModel : BaseViewModel
    {
        #region 字段
        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        private readonly List<byte> mReceivedSyncValues;

        private CancellationTokenSource mLoopCTS;
        private CancellationTokenSource mSyncCTS;
        private TaskCompletionSource<byte[]> mSyncTCS;
        #endregion

        #region 属性
        public ObservableCollection<LEDeviceViewModel> Devices { get; }
        public ObservableCollection<EndSymbol> EndSymbols { get; }

        public bool IsScanning
            => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
        public bool IsWriteOptionAvailable
            => WriteCharacteristic != null && WriteCharacteristic.IsWriteOptionAvailable && !IsWriting && LoopWriteState == LEWriteState.NotWriting && SyncWriteState == LEWriteState.NotWriting;

        private EndSymbol mEndSymbol;
        public EndSymbol EndSymbol
        {
            get { return mEndSymbol; }
            set { SetProperty(ref mEndSymbol, value); }
        }

        private LEDeviceViewModel mDevice;
        public LEDeviceViewModel Device
        {
            get { return mDevice; }
            set
            {
                var device = mDevice;
                if (!SetProperty(ref mDevice, value))
                    return;
                StopWriteCommands();
                if (device != null)
                {
                    device.RSSIChanged -= OnRSSIChanged;
                    device.Dispose();
                }
                if (mDevice != null)
                {
                    var array = BitConverter.GetBytes(mDevice.Address).Take(6).Reverse().ToArray();
                    var mac = BitConverter.ToString(array).Replace("-", string.Empty);
                    LELogger = new LELoggerViewModel(NavigationService, mac);
                    mDevice.RSSIChanged += OnRSSIChanged;
                }
                NotifyService = null;
                WriteService = null;
            }
        }

        private bool mIsLogRSSIEnabled;
        public bool IsLogRSSIEnabled
        {
            get { return mIsLogRSSIEnabled; }
            set { SetProperty(ref mIsLogRSSIEnabled, value); }
        }

        private LELoggerViewModel mLELogger;
        public LELoggerViewModel LELogger
        {
            get { return mLELogger; }
            set { SetProperty(ref mLELogger, value); }
        }

        private LEServiceViewModel mNotifyService;
        public LEServiceViewModel NotifyService
        {
            get { return mNotifyService; }
            set
            {
                if (!SetProperty(ref mNotifyService, value))
                    return;

                NotifyCharacteristic = null;
            }
        }

        private LEServiceViewModel mWriteService;
        public LEServiceViewModel WriteService
        {
            get { return mWriteService; }
            set
            {
                if (!SetProperty(ref mWriteService, value))
                    return;

                WriteCharacteristic = null;
            }
        }

        private LECharacteristicViewModel mNotifyCharacteristic;
        public LECharacteristicViewModel NotifyCharacteristic
        {
            get { return mNotifyCharacteristic; }
            set
            {
                var characteristic = mNotifyCharacteristic;
                if (!SetProperty(ref mNotifyCharacteristic, value))
                    return;
                StopWriteCommands();
                if (characteristic != null)
                {
                    characteristic.ValueChanged -= OnValueChanged;
                }
                if (mNotifyCharacteristic != null)
                {
                    mNotifyCharacteristic.ValueChanged += OnValueChanged;
                }
            }
        }

        private LECharacteristicViewModel mWriteCharacteristic;
        public LECharacteristicViewModel WriteCharacteristic
        {
            get { return mWriteCharacteristic; }
            set
            {
                if (!SetProperty(ref mWriteCharacteristic, value))
                    return;

                StopWriteCommands();
            }
        }

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

        private bool mIsLoopWrite;
        public bool IsLoopWrite
        {
            get { return mIsLoopWrite; }
            set { SetProperty(ref mIsLoopWrite, value); }
        }

        private int mInterval;
        public int Interval
        {
            get { return mInterval; }
            set { SetProperty(ref mInterval, value); }
        }

        private LEWriteState mLoopWriteState;
        public LEWriteState LoopWriteState
        {
            get { return mLoopWriteState; }
            set { SetProperty(ref mLoopWriteState, value); }
        }

        private LEWriteState mSyncWriteState;
        public LEWriteState SyncWriteState
        {
            get { return mSyncWriteState; }
            set { SetProperty(ref mSyncWriteState, value); }
        }
        #endregion

        #region 构造
        public LEViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            mWatcher = new BluetoothLEAdvertisementWatcher();
            mReceivedSyncValues = new List<byte>();
            mWatcher.Received += OnWatcherReceived;

            Devices = new ObservableCollection<LEDeviceViewModel>();
            var symbols = Enum.GetValues(typeof(EndSymbol)).Cast<EndSymbol>();
            EndSymbols = new ObservableCollection<EndSymbol>(symbols);
        }
        #endregion

        #region 方法
        private async void OnWatcherReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            await DispatcherRunAsync(() =>
            {
                var device = Devices.FirstOrDefault(i => i.Address == args.BluetoothAddress);
                if (device == null)
                {
                    device = new LEDeviceViewModel(NavigationService, args.BluetoothAddress, args.Advertisement.LocalName, args.RawSignalStrengthInDBm);
                    Devices.Add(device);
                }
                else
                {
                    device.RSSI = args.RawSignalStrengthInDBm;
                }
            });
        }

        private void OnRSSIChanged(object sender, EventArgs e)
        {
            if (!IsLogRSSIEnabled)
                return;
            LELogger.LogRSSI(Device.RSSI);
        }

        private void OnValueChanged(object sender, LECharacteristicValueEventArgs e)
        {
            CryptographicBuffer.CopyToByteArray(e.Value, out var value);
            if (SyncWriteState == LEWriteState.NotWriting)
            {
                LELogger.LogValueChanged(value);
            }
            else
            {
                mReceivedSyncValues.AddRange(value);
                var i = 0;
                while (i < mReceivedSyncValues.Count - 1)
                {
                    var j = i + 1;
                    if (mReceivedSyncValues[i] == 0x0D &&
                        mReceivedSyncValues[j] == 0x0A)
                        break;
                    i++;
                }
                var count = i + 2;
                if (count > mReceivedSyncValues.Count)
                    return;
                var received = mReceivedSyncValues.Take(count).ToArray();
                mReceivedSyncValues.RemoveRange(0, count);
                mSyncTCS.SetResult(received);
            }
        }

        private void StopWriteCommands()
        {
            if (StopLoopWriteCommand.CanExecute())
            {
                StopLoopWriteCommand.Execute();
            }
            if (StopSyncWriteCommand.CanExecute())
            {
                StopSyncWriteCommand.Execute();
            }
        }
        #endregion

        #region 命令

        private DelegateCommand mStartScanCommand;
        public DelegateCommand StartScanCommand =>
            mStartScanCommand ?? (mStartScanCommand = new DelegateCommand(ExecuteStartScanCommand, CanExecuteStartScanCommand).ObservesProperty(() => IsScanning));

        private bool CanExecuteStartScanCommand()
            => !IsScanning;

        void ExecuteStartScanCommand()
        {
            mWatcher.Start();
            RaisePropertyChanged(nameof(IsScanning));
        }

        private DelegateCommand mStopScanCommand;
        public DelegateCommand StopScanCommand =>
            mStopScanCommand ?? (mStopScanCommand = new DelegateCommand(ExecuteStopScanCommand, CanExecuteStopScanCommand).ObservesProperty(() => IsScanning));

        private bool CanExecuteStopScanCommand()
            => IsScanning;

        void ExecuteStopScanCommand()
        {
            mWatcher.Stop();
            RaisePropertyChanged(nameof(IsScanning));
        }

        private DelegateCommand mSwitchScanStateCommand;
        public DelegateCommand SwitchScanStateCommand =>
            mSwitchScanStateCommand ?? (mSwitchScanStateCommand = new DelegateCommand(ExecuteSwitchScanStateCommand));

        void ExecuteSwitchScanStateCommand()
        {
            if (IsScanning)
            {
                ExecuteStopScanCommand();
            }
            else
            {
                ExecuteStartScanCommand();
            }
        }

        private DelegateCommand mWriteCommand;
        public DelegateCommand WriteCommand =>
            mWriteCommand ?? (mWriteCommand = new DelegateCommand(ExecuteWriteCommand, CanExecuteWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState));

        private bool CanExecuteWriteCommand()
            => WriteCharacteristic != null && WriteCharacteristic.IsWritable && !string.IsNullOrWhiteSpace(Value) && !IsWriting && LoopWriteState == LEWriteState.NotWriting && SyncWriteState == LEWriteState.NotWriting;

        async void ExecuteWriteCommand()
        {
            IsWriting = true;
            var value = EndSymbol == EndSymbol.None
                      ? Encoding.UTF8.GetBytes($"{Value}")
                      : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
            var result = await WriteCharacteristic.WriteAsync(value);
            LELogger.LogWrite(value, result);
            IsWriting = false;
        }

        private DelegateCommand mStartLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            mStartLoopWriteCommand ?? (mStartLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => Interval));

        private bool CanExecuteStartLoopWriteCommand()
            => WriteCommand.CanExecute() && IsLoopWrite && Interval >= 0;

        async void ExecuteStartLoopWriteCommand()
        {
            LoopWriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            LELogger.LogLoopWriteStarted();
            using (var loopCTS = new CancellationTokenSource())
            {
                mLoopCTS = loopCTS;
                while (!loopCTS.Token.IsCancellationRequested)
                {
                    var value = Encoding.UTF8.GetBytes(Value);
                    var result = await WriteCharacteristic.WriteAsync(value);
                    LELogger.LogLoopWrite(value, result);
                    await Task.Delay(Interval);
                }
                LELogger.LogLoopWriteStopped();
                LoopWriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            }
        }

        private DelegateCommand mStopLoopWriteCommand;
        public DelegateCommand StopLoopWriteCommand =>
            mStopLoopWriteCommand ?? (mStopLoopWriteCommand = new DelegateCommand(ExecuteStopLoopWriteCommand, CanExecuteStopLoopWriteCommand).ObservesProperty(() => LoopWriteState));

        private bool CanExecuteStopLoopWriteCommand()
            => LoopWriteState == LEWriteState.Writing;

        void ExecuteStopLoopWriteCommand()
        {
            LoopWriteState = LEWriteState.StopWriting;
            mLoopCTS.Cancel();
            mLoopCTS = null;
        }

        private DelegateCommand mSwitchLoopWriteCommand;
        public DelegateCommand SwitchLoopWriteCommand =>
            mSwitchLoopWriteCommand ?? (mSwitchLoopWriteCommand = new DelegateCommand(ExecuteSwitchLoopWriteCommand, CanExecuteSwitchLoopWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState).ObservesProperty(() => IsLoopWrite).ObservesProperty(() => Interval));

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

        private DelegateCommand mStartSyncWriteCommand;
        public DelegateCommand StartSyncWriteCommand =>
            mStartSyncWriteCommand ?? (mStartSyncWriteCommand = new DelegateCommand(ExecuteStartSyncWriteCommand, CanExecuteStartSyncWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => IsWriting).ObservesProperty(() => LoopWriteState).ObservesProperty(() => SyncWriteState));

        private bool CanExecuteStartSyncWriteCommand()
            => WriteCommand.CanExecute();

        async void ExecuteStartSyncWriteCommand()
        {
            SyncWriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            LELogger.LogSyncWriteStarted();
            using (var syncCTS = new CancellationTokenSource())
            {
                mSyncCTS = syncCTS;
                while (!syncCTS.Token.IsCancellationRequested)
                {
                    var send = EndSymbol == EndSymbol.None
                             ? Encoding.UTF8.GetBytes($"{Value}")
                             : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
                    var received = await SyncWriteAsync(send);
                    LELogger.LogSyncWrite(send, received);
                }
                LELogger.LogSyncWriteStopped();
                SyncWriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            }
        }

        private async Task<byte[]> SyncWriteAsync(byte[] array)
        {
            mSyncTCS = new TaskCompletionSource<byte[]>();
            if (!await WriteCharacteristic.WriteAsync(array))
            {
                mSyncTCS.SetException(new IOException("写入失败"));
            }
            var value = await mSyncTCS.Task;
            return value;
        }

        private DelegateCommand mStopSyncWriteCommand;
        public DelegateCommand StopSyncWriteCommand =>
            mStopSyncWriteCommand ?? (mStopSyncWriteCommand = new DelegateCommand(ExecuteStopSyncWriteCommand, CanExecuteStopSyncWriteCommand).ObservesProperty(() => SyncWriteState));

        private bool CanExecuteStopSyncWriteCommand()
            => SyncWriteState == LEWriteState.Writing;

        void ExecuteStopSyncWriteCommand()
        {
            SyncWriteState = LEWriteState.StopWriting;
            mSyncCTS.Cancel();
            mSyncCTS = null;
        }
        #endregion
    }
}

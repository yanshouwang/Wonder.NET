using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography;

namespace Wonder.UWP.ViewModels
{
    public class LESyncWriteViewModel : BaseViewModel
    {
        #region 字段
        private readonly List<byte> mReceivedSyncValues;

        private CancellationTokenSource mSyncCTS;
        private TaskCompletionSource<byte[]> mSyncTCS;
        #endregion

        #region 属性
        public IList<EndSymbol> EndSymbols { get; }
        public bool IsWriteOptionAvailable
            => WriteCharacteristic != null && WriteCharacteristic.IsWriteOptionAvailable && WriteState == LEWriteState.NotWriting;

        private EndSymbol mEndSymbol;
        public EndSymbol EndSymbol
        {
            get { return mEndSymbol; }
            set { SetProperty(ref mEndSymbol, value); }
        }

        private LESyncWriteLoggerViewModel mLogger;
        public LESyncWriteLoggerViewModel Logger
        {
            get { return mLogger; }
            set { SetProperty(ref mLogger, value); }
        }

        private LEAdapterViewModel mAdapter;
        public LEAdapterViewModel Adapter
        {
            get { return mAdapter; }
            set { SetProperty(ref mAdapter, value); }
        }

        private LEDeviceViewModel mDevice;
        public LEDeviceViewModel Device
        {
            get { return mDevice; }
            set
            {
                if (Device != value && StopSyncWriteCommand.CanExecute())
                {
                    StopSyncWriteCommand.Execute();
                    Device.Dispose();
                    NotifyService = null;
                    WriteService = null;
                }
                SetProperty(ref mDevice, value);
            }
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
                if (NotifyCharacteristic != value && StopSyncWriteCommand.CanExecute())
                {
                    StopSyncWriteCommand.Execute();
                }
                SetProperty(ref mNotifyCharacteristic, value);
            }
        }

        private LECharacteristicViewModel mWriteCharacteristic;
        public LECharacteristicViewModel WriteCharacteristic
        {
            get { return mWriteCharacteristic; }
            set
            {
                if (WriteCharacteristic != value && StopSyncWriteCommand.CanExecute())
                {
                    StopSyncWriteCommand.Execute();
                }
                SetProperty(ref mWriteCharacteristic, value);
            }
        }

        private int mTimeout;
        public int Timeout
        {
            get { return mTimeout; }
            set { SetProperty(ref mTimeout, value); }
        }

        private string mValue;
        public string Value
        {
            get { return mValue; }
            set { SetProperty(ref mValue, value); }
        }

        private LEWriteState mWriteState;
        public LEWriteState WriteState
        {
            get { return mWriteState; }
            set { SetProperty(ref mWriteState, value); }
        }
        #endregion

        #region 构造
        public LESyncWriteViewModel(INavigationService navigationService, LEAdapterViewModel adapter)
            : base(navigationService)
        {
            mReceivedSyncValues = new List<byte>();

            Adapter = adapter;
            EndSymbols = Enum.GetValues(typeof(EndSymbol)).Cast<EndSymbol>().ToList();
            Timeout = 1000;
        }
        #endregion

        #region 方法
        private void OnValueChanged(object sender, LECharacteristicValueEventArgs e)
        {
            if (WriteState == LEWriteState.NotWriting)
            {
                throw new InvalidOperationException("未处理的异常");
            }
            CryptographicBuffer.CopyToByteArray(e.Value, out var value);
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
        #endregion

        #region 命令

        private DelegateCommand mStartSyncWriteCommand;
        public DelegateCommand StartSyncWriteCommand =>
            mStartSyncWriteCommand ?? (mStartSyncWriteCommand = new DelegateCommand(ExecuteStartSyncWriteCommand, CanExecuteStartSyncWriteCommand).ObservesProperty(() => NotifyCharacteristic).ObservesProperty(() => NotifyCharacteristic.CanNotify).ObservesProperty(() => NotifyCharacteristic.IsNotifying).ObservesProperty(() => WriteCharacteristic).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Timeout).ObservesProperty(() => Value).ObservesProperty(() => WriteState));

        private bool CanExecuteStartSyncWriteCommand()
            => NotifyCharacteristic != null && NotifyCharacteristic.CanNotify && !NotifyCharacteristic.IsNotifying && WriteCharacteristic != null && WriteCharacteristic.IsWritable && Timeout > 0 && !string.IsNullOrWhiteSpace(Value) && WriteState == LEWriteState.NotWriting;

        async void ExecuteStartSyncWriteCommand()
        {
            var array = BitConverter.GetBytes(mDevice.Address).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", string.Empty);
            Logger = new LESyncWriteLoggerViewModel(NavigationService, mac);

            await NotifyCharacteristic.StartNotifyAsync();
            if (!NotifyCharacteristic.IsNotifying)
                return;
            NotifyCharacteristic.ValueChanged += OnValueChanged;
            WriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            await Logger.LogStartedAsync();
            using (var syncCTS = new CancellationTokenSource())
            {
                mSyncCTS = syncCTS;
                while (!syncCTS.Token.IsCancellationRequested)
                {
                    var send = EndSymbol == EndSymbol.None
                             ? Encoding.UTF8.GetBytes($"{Value}")
                             : Encoding.UTF8.GetBytes($"{Value}{EndSymbol.ToValue()}");
                    var received = await SyncWriteAsync(send);
                    await Logger.LogWriteAsync(send, received);
                }
                await Logger.LogStoppedAsync();
                WriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
                NotifyCharacteristic.ValueChanged -= OnValueChanged;
                await NotifyCharacteristic.StopNotifyAsync();
            }
        }

        private async Task<byte[]> SyncWriteAsync(byte[] array)
        {
            mSyncTCS = new TaskCompletionSource<byte[]>();
            using (var cts = new CancellationTokenSource(Timeout))
            using (cts.Token.Register(() => OnSyncWriteTimeout()))
            {
                if (!await WriteCharacteristic.WriteAsync(array))
                {
                    mSyncTCS.SetException(new IOException("写入失败"));
                }
                try
                {
                    var value = await mSyncTCS.Task;
                    return value;
                }
                catch (Exception ex) when (ex is TimeoutException || ex is IOException)
                {
                    await Logger.LogAsync(ex.Message);
                    return null;
                }
            }
        }

        private void OnSyncWriteTimeout()
        {
            var ex = new TimeoutException("未在规定时间内收到通知，超时");
            mSyncTCS.SetException(ex);
        }

        private DelegateCommand mStopSyncWriteCommand;
        public DelegateCommand StopSyncWriteCommand =>
            mStopSyncWriteCommand ?? (mStopSyncWriteCommand = new DelegateCommand(ExecuteStopSyncWriteCommand, CanExecuteStopSyncWriteCommand).ObservesProperty(() => WriteState));

        private bool CanExecuteStopSyncWriteCommand()
            => WriteState == LEWriteState.Writing;

        void ExecuteStopSyncWriteCommand()
        {
            WriteState = LEWriteState.StopWriting;
            mSyncCTS.Cancel();
            mSyncCTS = null;
        }

        private DelegateCommand mSwitchSyncWriteCommand;
        public DelegateCommand SwitchSyncWriteCommand =>
            mSwitchSyncWriteCommand ?? (mSwitchSyncWriteCommand = new DelegateCommand(ExecuteSwitchSyncWriteCommand, CanExecuteSwitchSyncWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => WriteState));

        private bool CanExecuteSwitchSyncWriteCommand()
            => StartSyncWriteCommand.CanExecute() || StopSyncWriteCommand.CanExecute();

        void ExecuteSwitchSyncWriteCommand()
        {
            if (StartSyncWriteCommand.CanExecute())
            {
                StartSyncWriteCommand.Execute();
            }
            else
            {
                StopSyncWriteCommand.Execute();
            }
        }
        #endregion
    }
}

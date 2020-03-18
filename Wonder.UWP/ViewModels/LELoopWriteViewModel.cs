using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wonder.UWP.ViewModels
{
    public class LELoopWriteViewModel : BaseViewModel
    {
        #region 字段
        private CancellationTokenSource mCTS;
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

        private LELoopWriteLoggerViewModel mLogger;
        public LELoopWriteLoggerViewModel Logger
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
                if (Device != value && StopLoopWriteCommand.CanExecute())
                {
                    StopLoopWriteCommand.Execute();
                    Device.Dispose();
                    WriteService = null;
                }
                SetProperty(ref mDevice, value);
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

        private LECharacteristicViewModel mWriteCharacteristic;
        public LECharacteristicViewModel WriteCharacteristic
        {
            get { return mWriteCharacteristic; }
            set
            {
                if (WriteCharacteristic != value && StopLoopWriteCommand.CanExecute())
                {
                    StopLoopWriteCommand.Execute();
                }
                SetProperty(ref mWriteCharacteristic, value);
            }
        }

        private string mValue;
        public string Value
        {
            get { return mValue; }
            set { SetProperty(ref mValue, value); }
        }

        private int mInterval;
        public int Interval
        {
            get { return mInterval; }
            set { SetProperty(ref mInterval, value); }
        }

        private LEWriteState mWriteState;
        public LEWriteState WriteState
        {
            get { return mWriteState; }
            set { SetProperty(ref mWriteState, value); }
        }
        #endregion

        #region 构造
        public LELoopWriteViewModel(INavigationService navigationService, LEAdapterViewModel adapter)
            : base(navigationService)
        {
            Adapter = adapter;
            EndSymbols = Enum.GetValues(typeof(EndSymbol)).Cast<EndSymbol>().ToList();
        }
        #endregion

        #region 方法

        #endregion

        #region 命令

        private DelegateCommand mStartLoopWriteCommand;
        public DelegateCommand StartLoopWriteCommand =>
            mStartLoopWriteCommand ?? (mStartLoopWriteCommand = new DelegateCommand(ExecuteStartLoopWriteCommand, CanExecuteStartLoopWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => WriteState).ObservesProperty(() => Interval));

        private bool CanExecuteStartLoopWriteCommand()
            => WriteCharacteristic != null && WriteCharacteristic.IsWritable && !string.IsNullOrWhiteSpace(Value) && WriteState == LEWriteState.NotWriting && Interval >= 0;

        async void ExecuteStartLoopWriteCommand()
        {
            var array = BitConverter.GetBytes(mDevice.Address).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", string.Empty);
            Logger = new LELoopWriteLoggerViewModel(NavigationService, mac);

            WriteState = LEWriteState.Writing;
            RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            await Logger.LogStartedAsync();
            using (var loopCTS = new CancellationTokenSource())
            {
                mCTS = loopCTS;
                while (!loopCTS.Token.IsCancellationRequested)
                {
                    var value = Encoding.UTF8.GetBytes(Value);
                    var result = await WriteCharacteristic.WriteAsync(value);
                    await Logger.LogWriteAsync(value, result);
                    await Task.Delay(Interval);
                }
                await Logger.LogStoppedAsync();
                WriteState = LEWriteState.NotWriting;
                RaisePropertyChanged(nameof(IsWriteOptionAvailable));
            }
        }

        private DelegateCommand mStopLoopWriteCommand;
        public DelegateCommand StopLoopWriteCommand =>
            mStopLoopWriteCommand ?? (mStopLoopWriteCommand = new DelegateCommand(ExecuteStopLoopWriteCommand, CanExecuteStopLoopWriteCommand).ObservesProperty(() => WriteState));

        private bool CanExecuteStopLoopWriteCommand()
            => WriteState == LEWriteState.Writing;

        void ExecuteStopLoopWriteCommand()
        {
            WriteState = LEWriteState.StopWriting;
            mCTS.Cancel();
            mCTS = null;
        }

        private DelegateCommand mSwitchLoopWriteCommand;
        public DelegateCommand SwitchLoopWriteCommand =>
            mSwitchLoopWriteCommand ?? (mSwitchLoopWriteCommand = new DelegateCommand(ExecuteSwitchLoopWriteCommand, CanExecuteSwitchLoopWriteCommand).ObservesProperty(() => WriteCharacteristic.IsWritable).ObservesProperty(() => Value).ObservesProperty(() => WriteState).ObservesProperty(() => Interval));

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
        #endregion
    }
}

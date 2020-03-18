using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.ViewModels
{
    public class LERSSIViewModel : BaseViewModel
    {
        #region 字段

        #endregion

        #region 属性
        private LERSSILoggerViewModel mLogger;
        public LERSSILoggerViewModel Logger
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
                if (Device != value && StopLogCommand.CanExecute())
                {
                    StopLogCommand.Execute();
                }
                SetProperty(ref mDevice, value);
            }
        }

        private bool mIsLogging;
        public bool IsLogging
        {
            get { return mIsLogging; }
            set { SetProperty(ref mIsLogging, value); }
        }
        #endregion

        #region 构造
        public LERSSIViewModel(INavigationService navigationService, LEAdapterViewModel adapter)
            : base(navigationService)
        {
            Adapter = adapter;
        }
        #endregion

        #region 方法

        #endregion

        #region 命令
        private DelegateCommand mStartLogCommand;
        public DelegateCommand StartLogCommand =>
            mStartLogCommand ?? (mStartLogCommand = new DelegateCommand(ExecuteStartLogCommand, CanExecuteStartLogCommand).ObservesProperty(() => Device).ObservesProperty(() => IsLogging));

        private bool CanExecuteStartLogCommand()
            => Device != null && Device.ConnectionState == LEDeviceState.Disconnected && !IsLogging;

        async void ExecuteStartLogCommand()
        {
            var array = BitConverter.GetBytes(mDevice.Address).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", string.Empty);
            Logger = new LERSSILoggerViewModel(NavigationService, mac);

            IsLogging = true;
            Device.RSSIChanged += OnRSSIChanged;
            await Logger.LogStartedAsync();
        }

        private DelegateCommand mStopLogCommand;
        public DelegateCommand StopLogCommand =>
            mStopLogCommand ?? (mStopLogCommand = new DelegateCommand(ExecuteStopLogCommand, CanExecuteStopLogCommand).ObservesProperty(() => IsLogging));

        private bool CanExecuteStopLogCommand()
            => IsLogging;

        async void ExecuteStopLogCommand()
        {
            Device.RSSIChanged -= OnRSSIChanged;
            IsLogging = false;
            await Logger.LogStoppedAsync();
        }

        private async void OnRSSIChanged(object sender, EventArgs e)
        {
            await Logger.LogValueAsync(Device.RSSI);
        }
        #endregion
    }
}

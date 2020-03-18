using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Linq;
using Windows.Security.Cryptography;

namespace Wonder.UWP.ViewModels
{
    public class LEContinuousNotifyViewModel : BaseViewModel
    {
        #region 属性
        private bool mIsNotifying1;
        public bool IsNotifying1
        {
            get { return mIsNotifying1; }
            set { SetProperty(ref mIsNotifying1, value); }
        }

        private LEContinuousNotifyLoggerViewModel mLogger;
        public LEContinuousNotifyLoggerViewModel Logger
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
                if (Device != value && StopNotifyCommand.CanExecute())
                {
                    StopNotifyCommand.Execute();
                    Device.Dispose();
                    NotifyService = null;
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

        private LECharacteristicViewModel mNotifyCharacteristic;
        public LECharacteristicViewModel NotifyCharacteristic
        {
            get { return mNotifyCharacteristic; }
            set
            {
                if (NotifyCharacteristic != value && StopNotifyCommand.CanExecute())
                {
                    StopNotifyCommand.Execute();
                }
                SetProperty(ref mNotifyCharacteristic, value);
            }
        }
        #endregion

        #region 构造
        public LEContinuousNotifyViewModel(INavigationService navigationService, LEAdapterViewModel adapter)
            : base(navigationService)
        {
            Adapter = adapter;
        }
        #endregion

        #region 方法
        private async void OnValueChanged(object sender, LECharacteristicValueEventArgs e)
        {
            CryptographicBuffer.CopyToByteArray(e.Value, out var value);
            await Logger.LogNotifyAsync(value);
        }
        #endregion

        #region 命令
        private DelegateCommand mStartNotifyCommand;
        public DelegateCommand StartNotifyCommand =>
            mStartNotifyCommand ?? (mStartNotifyCommand = new DelegateCommand(ExecuteStartNotifyCommand, CanExecuteStartNotifyCommand).ObservesProperty(() => NotifyCharacteristic).ObservesProperty(() => NotifyCharacteristic.CanNotify).ObservesProperty(() => NotifyCharacteristic.IsNotifying).ObservesProperty(() => IsNotifying1));

        private bool CanExecuteStartNotifyCommand()
            => NotifyCharacteristic != null && NotifyCharacteristic.CanNotify && !NotifyCharacteristic.IsNotifying && !IsNotifying1;

        async void ExecuteStartNotifyCommand()
        {
            var array = BitConverter.GetBytes(mDevice.Address).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", string.Empty);
            Logger = new LEContinuousNotifyLoggerViewModel(NavigationService, mac);

            await NotifyCharacteristic.StartNotifyAsync();
            IsNotifying1 = NotifyCharacteristic.IsNotifying;
            if (IsNotifying1)
            {
                NotifyCharacteristic.ValueChanged += OnValueChanged;
                await Logger.LogStartedAsync();
            }
        }

        private DelegateCommand mStopNotifyCommand;
        public DelegateCommand StopNotifyCommand =>
            mStopNotifyCommand ?? (mStopNotifyCommand = new DelegateCommand(ExecuteStopNotifyCommand, CanExecuteStopNotifyCommand).ObservesProperty(() => IsNotifying1));

        private bool CanExecuteStopNotifyCommand()
            => IsNotifying1;

        async void ExecuteStopNotifyCommand()
        {
            await NotifyCharacteristic.StopNotifyAsync();
            IsNotifying1 = NotifyCharacteristic.IsNotifying;
            if (!IsNotifying1)
            {
                NotifyCharacteristic.ValueChanged -= OnValueChanged;
                await Logger.LogStoppedAsync();
            }
        }
        #endregion
    }
}

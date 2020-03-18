using Prism.Commands;
using Prism.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

namespace Wonder.UWP.ViewModels
{
    public class LEAdapterViewModel : BaseViewModel
    {
        #region 字段
        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        #endregion

        #region 属性
        public bool IsScanning
            => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
        public ObservableCollection<LEDeviceViewModel> Devices { get; }
        #endregion

        #region 构造
        public LEAdapterViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            mWatcher = new BluetoothLEAdvertisementWatcher();
            mWatcher.Received += OnWatcherReceived;

            Devices = new ObservableCollection<LEDeviceViewModel>();
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
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace Wonder.UWP.ViewModels
{
    public class LEViewModel : BaseViewModel
    {
        private readonly BluetoothLEAdvertisementWatcher _watcher;

        public ObservableCollection<LENode> Devices { get; }

        public bool IsScanning
            => _watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        public LEViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.Received += OnWatcherReceived;

            Devices = new ObservableCollection<LENode>();
        }

        private async void OnWatcherReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            await DispatcherRunAsync(() =>
            {
                var node = Devices.FirstOrDefault(i => i.Item is LEDeviceViewModel j && j.Address == args.BluetoothAddress);
                if (node == null)
                {
                    var device = new LEDeviceViewModel(NavigationService, args.BluetoothAddress, args.Advertisement.LocalName, args.RawSignalStrengthInDBm);
                    node = new LENode(device);
                    Devices.Add(node);
                }
                else
                {
                    var device = (LEDeviceViewModel)node.Item;
                    device.RSSI = args.RawSignalStrengthInDBm;
                }
            });
        }

        private DelegateCommand _startScanCommand;
        public DelegateCommand StartScanCommand =>
            _startScanCommand ?? (_startScanCommand = new DelegateCommand(ExecuteStartScanCommand, CanExecuteStartScanCommand).ObservesProperty(() => IsScanning));

        private bool CanExecuteStartScanCommand()
            => !IsScanning;

        void ExecuteStartScanCommand()
        {
            _watcher.Start();
            RaisePropertyChanged(nameof(IsScanning));
        }

        private DelegateCommand _stopScanCommand;
        public DelegateCommand StopScanCommand =>
            _stopScanCommand ?? (_stopScanCommand = new DelegateCommand(ExecuteStopScanCommand, CanExecuteStopScanCommand).ObservesProperty(() => IsScanning));

        private bool CanExecuteStopScanCommand()
            => IsScanning;

        void ExecuteStopScanCommand()
        {
            _watcher.Stop();
            RaisePropertyChanged(nameof(IsScanning));
        }
    }
}

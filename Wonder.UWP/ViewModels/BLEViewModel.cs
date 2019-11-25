using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Wonder.UWP.Constants;

namespace Wonder.UWP.ViewModels
{
    public class BLEViewModel : BaseViewModel
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEAdvertisementWatcher _watcher1;

        public ObservableCollection<BLEDeviceViewModel> Devices { get; set; }

        public BLEViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Devices = new ObservableCollection<BLEDeviceViewModel>();

            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(Guid.Parse(UUIDStrings.ATC_CUSTOM_SERVICE));
            _watcher.Received += OnWatcherReceived;
            _watcher1 = new BluetoothLEAdvertisementWatcher();
            var manufacturerData = new BluetoothLEManufacturerData { CompanyId = 0x5E04 };
            _watcher1.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
            _watcher1.Received += OnWatcher1Received;
            _watcher.Start();
            _watcher1.Start();
        }

        private async void OnWatcher1Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            if (Devices.Any(i => i.Address == args.BluetoothAddress))
                return;
            var datas = args.Advertisement.ManufacturerData;
            if (datas.Count == 0)
                return;
            var dataReader = DataReader.FromBuffer(datas[0].Data);
            if (dataReader.UnconsumedBufferLength < 9)
                return;
            var vid = datas[0].CompanyId;
            var dataArray = new byte[dataReader.UnconsumedBufferLength];
            dataReader.ReadBytes(dataArray);
            var value = BitConverter.ToString(dataArray).Replace("-", string.Empty);
            var keyArray = new byte[3];
            var macArray = new byte[6];
            Array.Copy(dataArray, 0, keyArray, 0, 3);
            Array.Copy(dataArray, 3, macArray, 0, 6);
            var key = BitConverter.ToString(keyArray).Replace("-", string.Empty);
            var mac = BitConverter.ToString(macArray).Replace("-", string.Empty);
            var name = args.Advertisement.LocalName;
            _watcher1.Stop();
            var item = Devices.FirstOrDefault(i => i.MAC == mac);
            if (item == null)
            {
                item = new BLEDeviceViewModel(
                    NavigationService,
                    Guid.Parse(UUIDStrings.APC2_0_COMMUNICATION_SERVICE),
                    Guid.Parse(UUIDStrings.APC2_0_NOTIFY_CHARACTERISTIC),
                    Guid.Parse(UUIDStrings.APC2_0_WRITE_CHARACTERISTIC),
                    args.BluetoothAddress,
                    name,
                    mac,
                    args.RawSignalStrengthInDBm);
                await DispatcherRunAsync(() => Devices.Add(item));
            }
            else
            {
                await DispatcherRunAsync(() => item.RSSI = args.RawSignalStrengthInDBm);
            }
            _watcher1.Start();
        }

        private DelegateCommand<BLEDeviceViewModel> _navigateCommand;
        public DelegateCommand<BLEDeviceViewModel> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<BLEDeviceViewModel>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(BLEDeviceViewModel device)
        {
            NavigationService.Navigate(ViewTokens.BLE_DEVICE, device);
        }

        private async void OnWatcherReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var mixed = args.Advertisement.LocalName;
            var pattern = @"(?<LETTER>(?i)(A|C))(?<NUMBER>\d{3})(?<MAC>[A-F|a-f|0-9]{4})";
            var match = Regex.Match(mixed, pattern);
            if (!match.Success)
                return;
            _watcher.Stop();
            var letter = match.Groups["LETTER"].Value.ToUpper();
            var number = match.Groups["NUMBER"].Value;
            var name = letter == "A" ? $"Additel {number}" : $"ConST {number}";
            var mac = match.Groups["MAC"].Value;
            var item = Devices.FirstOrDefault(i => i.MAC == mac);
            if (item == null)
            {
                item = new BLEDeviceViewModel(
                    NavigationService,
                    Guid.Parse(UUIDStrings.ATC_COMMUNICATION_SERVICE),
                    Guid.Parse(UUIDStrings.ATC_NOTIFY_CHARACTERISTIC),
                    Guid.Parse(UUIDStrings.ATC_WRITE_CHARACTERISTIC),
                    args.BluetoothAddress,
                    name,
                    mac,
                    args.RawSignalStrengthInDBm);
                await DispatcherRunAsync(() => Devices.Add(item));
            }
            else
            {
                await DispatcherRunAsync(() => item.RSSI = args.RawSignalStrengthInDBm);
            }
            _watcher.Start();
        }
    }
}

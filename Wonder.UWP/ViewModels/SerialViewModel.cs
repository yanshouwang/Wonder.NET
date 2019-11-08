using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Wonder.UWP.ViewModels
{
    public class SerialViewModel : BaseViewModel
    {
        public ObservableCollection<DeviceViewModel> Devices { get; set; }

        public SerialViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Devices = new ObservableCollection<DeviceViewModel>();

            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            var selector = SerialDevice.GetDeviceSelector();
            // 获取已连接设备
            //var items = await DeviceInformation.FindAllAsync(selector);
            //foreach (var item in items)
            //{
            //    var device = new DeviceViewModel(item.Id, item.Name);
            //    Devices.Add(device);
            //}
            // 监听设备
            var watcher = DeviceInformation.CreateWatcher(selector);
            watcher.Added += OnDeviceAdded;
            watcher.Removed += OnDeviceRemoved;
            watcher.Updated += OnDeviceUpdated;
            watcher.EnumerationCompleted += OnWatcherEnumerationCompleted;
            watcher.Stopped += OnWatcherStoped;
            watcher.Start();
        }

        private void OnWatcherStoped(DeviceWatcher sender, object args)
        {
            throw new NotImplementedException();
        }

        private void OnWatcherEnumerationCompleted(DeviceWatcher sender, object args)
        {

        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            throw new NotImplementedException();
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var device = Devices.Single(i => i.ID == args.Id);
            RunOnUI(() => Devices.Remove(device));
        }

        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            var device = new DeviceViewModel(args.Id, args.Name);
            RunOnUI(() => Devices.Add(device));
        }
    }
}

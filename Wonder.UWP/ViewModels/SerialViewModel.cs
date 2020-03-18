using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Wonder.UWP.IO;

namespace Wonder.UWP.ViewModels
{
    public class SerialViewModel : BaseViewModel
    {
        private readonly DeviceWatcher mWatcher;

        public ObservableCollection<SerialDeviceViewModel> Devices { get; }

        public SerialViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Devices = new ObservableCollection<SerialDeviceViewModel>();
            var selector = SerialDevice.GetDeviceSelector();
            mWatcher = DeviceInformation.CreateWatcher(selector);
            mWatcher.Added += OnDeviceAdded;
            mWatcher.Removed += OnDeviceRemoved;
            mWatcher.Updated += OnDeviceUpdated;
            mWatcher.EnumerationCompleted += OnWatcherEnumerationCompleted;
            mWatcher.Stopped += OnWatcherStoped;
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);
            mWatcher.Start();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
            mWatcher.Stop();
        }

        private void OnWatcherStoped(DeviceWatcher sender, object args)
        {

        }

        private void OnWatcherEnumerationCompleted(DeviceWatcher sender, object args)
        {

        }

        private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            throw new NotImplementedException();
        }

        private async void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var device = Devices.Single(i => i.ID == args.Id);
            await DispatcherRunAsync(() => Devices.Remove(device));
        }

        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            var portNames = Serial.GetPortNames();

            foreach (var portName in portNames)
            {
                var selector = SerialDevice.GetDeviceSelector(portName);
                var items = await DeviceInformation.FindAllAsync(selector);
                if (items.All(i => i.Id != args.Id))
                    continue;
                var device = new SerialDeviceViewModel(portName, args.Id, args.Name);
                await DispatcherRunAsync(() =>
                {
                    if (Devices.Any(i => i.ID == device.ID))
                        return;
                    Devices.Add(device);
                });
            }
        }
    }
}

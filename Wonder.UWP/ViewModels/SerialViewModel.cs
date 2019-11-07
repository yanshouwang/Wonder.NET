using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace Wonder.UWP.ViewModels
{
    public class SerialViewModel : BaseViewModel
    {
        public SerialViewModel(INavigationService navigationService)
            : base(navigationService)
        {

        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(e, viewModelState);

            var filter = SerialDevice.GetDeviceSelector();
            var array = await DeviceInformation.FindAllAsync(filter);
            foreach (var item in array)
            {
                var device = await SerialDevice.FromIdAsync(item.Id);
            }
        }
    }
}

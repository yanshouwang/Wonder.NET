using System;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

namespace Wonder.UWP.Extension
{
    static class BLEExtensions
    {
        public static string Address2MAC(this BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var array = BitConverter.GetBytes(args.BluetoothAddress).Take(6).Reverse().ToArray();
            var mac = BitConverter.ToString(array).Replace("-", ":");
            return mac;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;

namespace Wonder.UWP.Services
{
    class BLEMonitorService : BaseMonitorService
    {
        public const string SERVICE_UUID = "AF661820-D14A-4B21-90F8-54D58F8614F0";
        public const string CHARACTERISTIC_UUID = "1B6B9415-FF0D-47C2-9444-A5032F727B2D";

        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        private readonly IList<ulong> mAddresses;
        private readonly IDictionary<ulong, BluetoothLEDevice> mDevices;
        private readonly IDictionary<ulong, GattDeviceService> mServices;
        private readonly IDictionary<ulong, GattCharacteristic> mCharacteristics;
        //private readonly SemaphoreSlim mDevicesSlim;

        public BLEMonitorService()
        {
            var filter = new BluetoothLEAdvertisementFilter();
            var item = Guid.Parse(SERVICE_UUID);
            filter.Advertisement.ServiceUuids.Add(item);
            mWatcher = new BluetoothLEAdvertisementWatcher(filter);
            mAddresses = new List<ulong>();
            mDevices = new Dictionary<ulong, BluetoothLEDevice>();
            mServices = new Dictionary<ulong, GattDeviceService>();
            mCharacteristics = new Dictionary<ulong, GattCharacteristic>();
            //mDevicesSlim = new SemaphoreSlim(1, 1);
            mWatcher.Received += OnWatcherReceived;
        }

        private async void OnWatcherReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // TODO: 分辨设备
            if (mAddresses.Contains(args.BluetoothAddress))
                return;
            mAddresses.Add(args.BluetoothAddress);
            await CommunicateAsync(args.BluetoothAddress);
        }

        private async Task CommunicateAsync(ulong address)
        {
            try
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                //device.ConnectionStatusChanged += OnConnectionStatusChanged;
                mDevices.Add(address, device);
                var serviceUuid = Guid.Parse(SERVICE_UUID);
                var sr = await device.GetGattServicesForUuidAsync(serviceUuid);
                if (sr.Status == GattCommunicationStatus.Success)
                {
                    var service = sr.Services[0];
                    mServices.Add(address, service);
                    var characteristicUuid = Guid.Parse(CHARACTERISTIC_UUID);
                    var cr = await service.GetCharacteristicsForUuidAsync(characteristicUuid);
                    if (cr.Status == GattCommunicationStatus.Success)
                    {
                        var characteristic = cr.Characteristics[0];
                        mCharacteristics.Add(address, characteristic);
                        var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        if (status == GattCommunicationStatus.Success)
                        {
                            characteristic.ValueChanged += OnValueChanged;
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException)
            {

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var value);
            RaiseValueChanged(value);
        }

        protected override Task<bool> TryStartAsync()
        {
            mWatcher.Start();
            return Task.FromResult(true);
        }

        protected override async Task TrySendAsync(byte[] data)
        {
            foreach (var characteristic in mCharacteristics.Values)
            {
                var canWrite = characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write);
                var canWriteWithoutResponse = characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);
                if (!canWrite && !canWriteWithoutResponse)
                    return;
                var option = canWriteWithoutResponse ? GattWriteOption.WriteWithoutResponse : GattWriteOption.WriteWithResponse;
                // 大于 20 字节分包发送（最大可以支持 244 字节）
                // https://stackoverflow.com/questions/53313117/cannot-write-large-byte-array-to-a-ble-device-using-uwp-apis-e-g-write-value
                var count = data.Length / 20;
                var remainder = data.Length % 20;
                var carriage = new byte[20];
                for (int i = 0; i < count; i++)
                {
                    Array.Copy(data, i * 20, carriage, 0, 20);
                    var value = CryptographicBuffer.CreateFromByteArray(carriage);
                    var status = await characteristic.WriteValueAsync(value, option);
                    //var result = await characteristic.WriteValueWithResultAsync(value, option);
                    //var status = result.Status;
                    if (status != GattCommunicationStatus.Success)
                    {
                        //TODO: 是否需要重试
                    }
                }
                if (remainder > 0)
                {
                    carriage = new byte[remainder];
                    Array.Copy(data, count * 20, carriage, 0, remainder);
                    var value = CryptographicBuffer.CreateFromByteArray(carriage);
                    var status = await characteristic.WriteValueAsync(value, option);
                    if (status != GattCommunicationStatus.Success)
                    {
                        //TODO: 是否需要重试
                    }
                }
            }
        }

        protected override bool TryStop()
        {
            mWatcher.Stop();
            mCharacteristics.Clear();
            foreach (var service in mServices.Values)
            {
                service.Dispose();
            }
            mServices.Clear();
            foreach (var device in mDevices.Values)
            {
                device.Dispose();
            }
            mDevices.Clear();
            mAddresses.Clear();
            return true;
        }
    }
}

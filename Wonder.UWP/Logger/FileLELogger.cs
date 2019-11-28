using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Wonder.UWP.Logger
{
    public class FileLELogger : ILELogger
    {
        private const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);
        private const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        private const int ERROR_UNABLE_TO_REMOVE_REPLACED = unchecked((int)0x80070497);
        private const int RETRY_ATTEMPTS = 5;

        private readonly string _fileName1;
        private readonly string _fileName2;
        private readonly IList<int> _rssis;
        private readonly SemaphoreSlim _fileSemaphore;

        private int _sendCount;
        private int _failedCount;
        private int _receivedCount;
        private long _sendLength;
        private long _failedLength;
        private long _receivedLength;

        public FileLELogger(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("日志文件名称不能为空", nameof(fileName));
            }

            var time = DateTime.Now.ToFileTime();
            _fileName1 = $"{fileName}-{time}-日志";
            _fileName2 = $"{fileName}-{time}-统计";
            _rssis = new List<int>();
            _fileSemaphore = new SemaphoreSlim(1, 1);
        }

        public async void LogRSSI(int rssi)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var file1 = await folder.CreateFileAsync(_fileName1, CreationCollisionOption.OpenIfExists);
                _rssis.Add(rssi);
                var content1 = $"{DateTime.Now} >>> RSSI：{rssi}\r\n";
                var retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.AppendTextAsync(file1, content1);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
                var file2 = await folder.CreateFileAsync(_fileName2, CreationCollisionOption.ReplaceExisting);
                var content2 = $"RSSI: 平均值->{(int)_rssis.Average()} 最大值->{_rssis.Max()} 最小值->{_rssis.Min()}\r\n" +
                               $"接收：{_receivedCount}包 {_receivedLength}字节\r\n" +
                               $"发送成功{_sendCount}包 {_sendLength}字节\r\n" +
                               $"发送失败{_failedCount}包 {_failedLength}字节";
                retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(file2, content2);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public async void LogReceived(byte[] value)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var file1 = await folder.CreateFileAsync(_fileName1, CreationCollisionOption.OpenIfExists);
                _receivedCount++;
                _receivedLength += value.Length;
                var content1 = $"{DateTime.Now} >>> 接收：{BitConverter.ToString(value)}\r\n";
                var retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.AppendTextAsync(file1, content1);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
                var file2 = await folder.CreateFileAsync(_fileName2, CreationCollisionOption.ReplaceExisting);
                int average = 0, maximum = 0, minimum = 0;
                if (_rssis.Any())
                {
                    average = (int)_rssis.Average();
                    maximum = _rssis.Max();
                    minimum = _rssis.Min();
                }
                var content2 = $"RSSI: 平均值->{average} 最大值->{maximum} 最小值->{minimum}\r\n" +
                               $"接收：{_receivedCount}包 {_receivedLength}字节\r\n" +
                               $"发送成功{_sendCount}包 {_sendLength}字节\r\n" +
                               $"发送失败{_failedCount}包 {_failedLength}字节";
                retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(file2, content2);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }

        public async void LogSend(byte[] value, bool result)
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var file1 = await folder.CreateFileAsync(_fileName1, CreationCollisionOption.OpenIfExists);
                string what;
                if (result)
                {
                    _sendCount++;
                    _sendLength += value.Length;
                    what = "发送成功";
                }
                else
                {
                    _failedCount++;
                    _failedLength += value.Length;
                    what = "发送失败";
                }
                var content1 = $"{DateTime.Now} >>> {what}：{BitConverter.ToString(value)}\r\n";
                var retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.AppendTextAsync(file1, content1);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
                var file2 = await folder.CreateFileAsync(_fileName2, CreationCollisionOption.ReplaceExisting);
                int average = 0, maximum = 0, minimum = 0;
                if (_rssis.Any())
                {
                    average = (int)_rssis.Average();
                    maximum = _rssis.Max();
                    minimum = _rssis.Min();
                }
                var content2 = $"RSSI: 平均值->{average} 最大值->{maximum} 最小值->{minimum}\r\n" +
                               $"接收：{_receivedCount}包 {_receivedLength}字节\r\n" +
                               $"发送成功{_sendCount}包 {_sendLength}字节\r\n" +
                               $"发送失败{_failedCount}包 {_failedLength}字节";
                retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(file2, content2);
                        break;
                    }
                    catch (Exception ex) when (ex.HResult == ERROR_ACCESS_DENIED ||
                                               ex.HResult == ERROR_SHARING_VIOLATION ||
                                               ex.HResult == ERROR_UNABLE_TO_REMOVE_REPLACED)
                    {
                        // This might be recovered by retrying, otherwise let the exception be raised.
                        // The app can decide to wait before retrying.
                    }
                }
            }
            finally
            {
                _fileSemaphore.Release();
            }
        }
    }
}

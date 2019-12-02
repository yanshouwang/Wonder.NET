using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Wonder.UWP.Logger
{
    public class FileLELoggerX : MemoryLELoggerX
    {
        private const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);
        private const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        private const int ERROR_UNABLE_TO_REMOVE_REPLACED = unchecked((int)0x80070497);
        private const int RETRY_ATTEMPTS = 5;

        private readonly string _fileName1;
        private readonly string _fileName2;
        private readonly SemaphoreSlim _fileSemaphore;

        public FileLELoggerX(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                throw new ArgumentException("日志文件名称不能为空", nameof(mac));
            }

            var time = DateTime.Now.ToFileTime();
            _fileName1 = $"{mac}-{time}-日志.txt";
            _fileName2 = $"{mac}-{time}-统计.txt";
            _fileSemaphore = new SemaphoreSlim(1, 1);
        }

        protected override async void HandleRSSI(int rssi)
        {
            base.HandleRSSI(rssi);
            await LogAsync();
            await CensusAsync();
        }

        protected override async void HandleValueChanged(byte[] value)
        {
            base.HandleValueChanged(value);
            await LogAsync();
            await CensusAsync();
        }

        protected override async void HandleWrite(byte[] value, bool result)
        {
            base.HandleWrite(value, result);
            await LogAsync();
            await CensusAsync();
        }

        protected override async void HandleLoopWriteStarted()
        {
            base.HandleLoopWriteStarted();
            await LogAsync();
        }

        protected override async void HandleLoopWrite(byte[] value, bool result)
        {
            base.HandleLoopWrite(value, result);
            await LogAsync();
            await CensusAsync();
        }

        protected override async void HandleLoopWriteStopped()
        {
            base.HandleLoopWriteStopped();
            await LogAsync();
        }

        private async Task LogAsync()
        {
            var log = Logs[0];
            await _fileSemaphore.WaitAsync();
            try
            {
                var folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(_fileName1, CreationCollisionOption.OpenIfExists);
                var content = $"{log.Time} -> {log.Message}\r\n";
                var retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.AppendTextAsync(file, content);
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

        private async Task CensusAsync()
        {
            await _fileSemaphore.WaitAsync();
            try
            {
                var folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(_fileName2, CreationCollisionOption.ReplaceExisting);
                var content = $"***RSSI***\r\n" +
                              $"平均值->{AverageRSSI} 最大值->{MaximumRSSI} 最小值->{MinimumRSSI}\r\n" +
                              $"***统计***\r\n" +
                              $"发送成功：{WriteSucceedCount}包 {WriteSucceedLength}字节\r\n" +
                              $"发送失败：{WriteFailedCount}包 {WriteFailedLength}字节\r\n" +
                              $"接收成功：{ValueChangedCount}包 {ValueChangedLength}字节\r\n" +
                              $"***测试***\r\n" +
                              $"起止时间：{LoopWriteStartedTime} - {LoopWriteStoppedTime}\r\n" +
                              $"发送成功：{LoopWriteSucceedCount}包 {LoopWriteSucceedLength}字节\r\n" +
                              $"发送失败：{LoopWriteFailedCount}包 {LoopWriteFailedLength}字节\r\n" +
                              $"平均速度：{LoopWriteSpeed}字节/秒";
                var retryAttempts = RETRY_ATTEMPTS;
                while (retryAttempts > 0)
                {
                    try
                    {
                        retryAttempts--;
                        await FileIO.WriteTextAsync(file, content);
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

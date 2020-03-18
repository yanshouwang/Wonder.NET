using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Wonder.UWP.Logger
{
    internal class StorageLogger : ILogger
    {
        private const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);
        private const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        private const int ERROR_UNABLE_TO_REMOVE_REPLACED = unchecked((int)0x80070497);
        private const int RETRY_ATTEMPTS = 5;

        private readonly string mFileName;
        private readonly SemaphoreSlim mFileSemaphore;

        public StorageLogger(string fileName)
        {
            mFileName = fileName;
            mFileSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LogAsync(string message, LogMode mode = LogMode.All)
        {
            await mFileSemaphore.WaitAsync();
            try
            {
                var folder = await KnownFolders.DocumentsLibrary.CreateFolderAsync("Wonder", CreationCollisionOption.OpenIfExists);
                var options = mode.ToCreationCollisionOption();
                var file = await folder.CreateFileAsync(mFileName, options);
                var content = $"{DateTime.Now} -> {message}\r\n";
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
                mFileSemaphore.Release();
            }
        }
    }
}

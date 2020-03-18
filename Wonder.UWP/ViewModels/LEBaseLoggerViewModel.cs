using Prism.Windows.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;
using Wonder.UWP.Models;

namespace Wonder.UWP.ViewModels
{
    public class LEBaseLoggerViewModel : BaseViewModel
    {
        #region 属性
        protected ILogger LogsLogger { get; }
        protected ILogger CensusLogger { get; }

        public ObservableCollection<LogModel> Logs { get; }
        #endregion

        #region 构造
        public LEBaseLoggerViewModel(INavigationService navigationService, string name)
            : base(navigationService)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("日志文件名称不能为空", nameof(name));
            }

            var time = DateTime.Now.ToFileTime();
            var fileName1 = $"{name}-{time}-日志.txt";
            var fileName2 = $"{name}-{time}-统计.txt";
            LogsLogger = new StorageLogger(fileName1);
            CensusLogger = new StorageLogger(fileName2);
            Logs = new ObservableCollection<LogModel>();
        }
        #endregion

        #region 方法
        public async Task LogAsync(string message)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Add(message));
            await LogAsync();
        }

        protected void Add(string message)
        {
            var log = new LogModel(message);
            Logs.Insert(0, log);
        }

        protected async Task LogAsync()
        {
            var message = Logs[0].Message;
            await LogsLogger.LogAsync(message);
        }
        #endregion
    }
}

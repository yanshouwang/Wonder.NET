using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Windows.Navigation;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Wonder.UWP.ViewModels
{
    public class LEServiceViewModel : BaseViewModel, IDisposable
    {
        #region 字段
        private readonly GattDeviceService mService;
        #endregion

        #region 属性
        public Guid UUID
            => mService.Uuid;

        public ObservableCollection<LECharacteristicViewModel> Characteristics { get; }
        #endregion

        #region 构造
        public LEServiceViewModel(INavigationService navigationService, GattDeviceService service, IList<LECharacteristicViewModel> characteristics)
            : base(navigationService)
        {
            mService = service;
            Characteristics = characteristics == null
                            ? new ObservableCollection<LECharacteristicViewModel>()
                            : new ObservableCollection<LECharacteristicViewModel>(characteristics);
        }
        #endregion

        #region IDisposable Support
        private bool mDisposed = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    foreach (var characteristic in Characteristics)
                    {
                        if (characteristic.StopNotifyCommand.CanExecute())
                        {
                            characteristic.StopNotifyCommand.Execute();
                        }
                    }
                    mService.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                mDisposed = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~LEServiceViewModel()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

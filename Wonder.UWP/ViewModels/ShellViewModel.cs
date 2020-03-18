using Prism.Commands;
using Prism.Windows.Navigation;
using System;

namespace Wonder.UWP.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        private LERSSIViewModel mRSSI;
        public LERSSIViewModel RSSI
        {
            get { return mRSSI; }
            set { SetProperty(ref mRSSI, value); }
        }

        private LELoopWriteViewModel mLoopWrite;
        public LELoopWriteViewModel LoopWrite
        {
            get { return mLoopWrite; }
            set { SetProperty(ref mLoopWrite, value); }
        }

        private LESyncWriteViewModel mSyncWrite;
        public LESyncWriteViewModel SyncWrite
        {
            get { return mSyncWrite; }
            set { SetProperty(ref mSyncWrite, value); }
        }

        private LEContinuousNotifyViewModel mContinuousNotify;
        public LEContinuousNotifyViewModel ContinuousNotify
        {
            get { return mContinuousNotify; }
            set { SetProperty(ref mContinuousNotify, value); }
        }

        public ShellViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            var adapter = new LEAdapterViewModel(navigationService);
            RSSI = new LERSSIViewModel(navigationService, adapter);
            LoopWrite = new LELoopWriteViewModel(navigationService, adapter);
            SyncWrite = new LESyncWriteViewModel(navigationService, adapter);
            ContinuousNotify = new LEContinuousNotifyViewModel(navigationService, adapter);
        }

        private DelegateCommand<string> mNavigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            mNavigateCommand ?? (mNavigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(string viewToken)
        {
            if (NavigationService.Navigate(viewToken, null))
                return;

            throw new NotImplementedException();
        }

        private DelegateCommand mGoBackCommand;
        public DelegateCommand GoBackCommand =>
            mGoBackCommand ?? (mGoBackCommand = new DelegateCommand(ExecuteGoBackCommand));

        void ExecuteGoBackCommand()
        {
            if (!NavigationService.CanGoBack())
                return;

            NavigationService.GoBack();
        }
    }
}

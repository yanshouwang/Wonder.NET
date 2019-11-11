﻿using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Wonder.UWP.Constants;
using Wonder.UWP.Services;
using Wonder.UWP.Xaml;

namespace Wonder.UWP.ViewModels
{
    public class ShellViewModel : BaseViewModel
    {
        public ShellViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }

        private DelegateCommand<string> _navigateCommand;
        public DelegateCommand<string> NavigateCommand =>
            _navigateCommand ?? (_navigateCommand = new DelegateCommand<string>(ExecuteNavigateCommand));

        void ExecuteNavigateCommand(string viewToken)
        {
            if (NavigationService.Navigate(viewToken, null))
                return;

            throw new NotImplementedException();
        }

        private DelegateCommand _gobackCommand;
        public DelegateCommand GoBackCommand =>
            _gobackCommand ?? (_gobackCommand = new DelegateCommand(ExecuteGoBackCommand));

        void ExecuteGoBackCommand()
        {
            if (!NavigationService.CanGoBack())
                return;

            NavigationService.GoBack();
        }
    }
}

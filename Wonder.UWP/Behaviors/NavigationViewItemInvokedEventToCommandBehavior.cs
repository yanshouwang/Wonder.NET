using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Wonder.UWP.Helpers;

namespace Wonder.UWP.Behaviors
{
    public class NavigationViewItemInvokedEventToCommandBehavior
        : Behavior<NavigationView>
    {
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="Command"/>.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(NavigationViewItemInvokedEventToCommandBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemInvoked += OnItemInvoked;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ItemInvoked -= OnItemInvoked;
            base.OnDetaching();
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (Command == null)
                return;
            var viewToken = args.IsSettingsInvoked
                          ? "SettingsView"
                          : NavHelper.GetViewToken(args.InvokedItemContainer);
            if (!Command.CanExecute(viewToken))
                return;
            Command.Execute(viewToken);
        }
    }
}

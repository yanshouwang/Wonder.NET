using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Wonder.UWP.ViewModels;

using MUXC = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Wonder.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LEView : Page
    {
        public LEViewModel ViewModel
            => DataContext as LEViewModel;

        public ObservableCollection<LENode> Nodes { get; }

        public LENode SelectedNode { get; set; }

        public LEView()
        {
            this.InitializeComponent();
            Nodes = new ObservableCollection<LENode>();
            ViewModel.Devices.CollectionChanged += OnDevicesChanged;
        }

        private void OnDevicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (!(e.NewItems[0] is LEDeviceViewModel added))
                            return;
                        added.Services.CollectionChanged += OnServicesChanged;
                        Nodes.Add(new LENode(added));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        Nodes.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (!(e.OldItems[0] is LEDeviceViewModel removed))
                            return;
                        var removedNode = Nodes.FirstOrDefault(i => i.Value == removed);
                        if (removedNode == null)
                            return;
                        removed.Services.CollectionChanged -= OnServicesChanged;
                        Nodes.Remove(removedNode);
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (!(e.OldItems[0] is LEDeviceViewModel previous) ||
                            !(e.NewItems[0] is LEDeviceViewModel current))
                            return;
                        var previousNode = Nodes.FirstOrDefault(i => i.Value == previous);
                        if (previousNode == null)
                            return;
                        var previousIndex = Nodes.IndexOf(previousNode);
                        previous.Services.CollectionChanged -= OnServicesChanged;
                        current.Services.CollectionChanged += OnServicesChanged;
                        Nodes[previousIndex] = new LENode(current);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (var node in Nodes)
                        {
                            if (!(node.Value is LEDeviceViewModel cleared))
                                continue;
                            cleared.Services.CollectionChanged -= OnServicesChanged;
                        }
                        Nodes.Clear();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void OnServicesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var deviceNode = Nodes.FirstOrDefault(i => (i.Value as LEDeviceViewModel).Services == sender);
            if (deviceNode == null)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (!(e.NewItems[0] is LEServiceViewModel added))
                            return;
                        added.Characteristics.CollectionChanged += OnCharacteristicsChanged;
                        var serviceNode = new LENode(added);
                        foreach (var characteristic in added.Characteristics)
                        {
                            var characteristicNode = new LENode(characteristic);
                            serviceNode.Nodes.Add(characteristicNode);
                        }
                        deviceNode.Nodes.Add(serviceNode);
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        deviceNode.Nodes.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (!(e.OldItems[0] is LEServiceViewModel removed))
                            return;
                        var removedNode = deviceNode.Nodes.FirstOrDefault(i => i.Value == removed);
                        if (removedNode == null)
                            return;
                        removed.Characteristics.CollectionChanged -= OnCharacteristicsChanged;
                        deviceNode.Nodes.Remove(removedNode);
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (!(e.OldItems[0] is LEServiceViewModel previous) ||
                            !(e.NewItems[0] is LEServiceViewModel current))
                            return;
                        var previousNode = deviceNode.Nodes.FirstOrDefault(i => i.Value == previous);
                        if (previousNode == null)
                            return;
                        var previousIndex = deviceNode.Nodes.IndexOf(previousNode);
                        previous.Characteristics.CollectionChanged -= OnCharacteristicsChanged;
                        current.Characteristics.CollectionChanged += OnCharacteristicsChanged;
                        var serviceNode = new LENode(current);
                        foreach (var characteristic in current.Characteristics)
                        {
                            var characteristicNode = new LENode(characteristic);
                            serviceNode.Nodes.Add(characteristicNode);
                        }
                        deviceNode.Nodes[previousIndex] = serviceNode;
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (var node in deviceNode.Nodes)
                        {
                            if (!(node.Value is LEServiceViewModel cleared))
                                continue;
                            cleared.Characteristics.CollectionChanged -= OnCharacteristicsChanged;
                        }
                        deviceNode.Nodes.Clear();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void OnCharacteristicsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var serviceNode = Nodes.SelectMany(i => i.Nodes).FirstOrDefault(i => (i.Value as LEServiceViewModel).Characteristics == sender);
            if (serviceNode == null)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (!(e.NewItems[0] is LECharacteristicViewModel added))
                            return;
                        serviceNode.Nodes.Add(new LENode(added));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        serviceNode.Nodes.Move(e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (!(e.OldItems[0] is LECharacteristicViewModel removed))
                            return;
                        var removedNode = serviceNode.Nodes.FirstOrDefault(i => i.Value == removed);
                        if (removedNode == null)
                            return;
                        serviceNode.Nodes.Remove(removedNode);
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (!(e.OldItems[0] is LECharacteristicViewModel previous) ||
                            !(e.NewItems[0] is LECharacteristicViewModel current))
                            return;
                        var previousNode = serviceNode.Nodes.FirstOrDefault(i => i.Value == previous);
                        if (previousNode == null)
                            return;
                        var previousIndex = serviceNode.Nodes.IndexOf(previousNode);
                        serviceNode.Nodes[previousIndex] = new LENode(current);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        serviceNode.Nodes.Clear();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void ConnectBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedNode?.Value is LEDeviceViewModel device) ||
                !device.ConnectComamnd.CanExecute())
                return;
            if (ViewModel.StopScanCommand.CanExecute())
                ViewModel.StopScanCommand.Execute();
            device.ConnectComamnd.Execute();
        }

        private void DevicesTVW_ItemInvoked(MUXC.TreeView sender, MUXC.TreeViewItemInvokedEventArgs args)
        {
            SelectedNode = (LENode)args.InvokedItem;
        }
    }
}

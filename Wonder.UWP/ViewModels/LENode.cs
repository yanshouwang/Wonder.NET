using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.ViewModels
{
    public class LENode
    {
        public object Item { get; set; }
        public ObservableCollection<LENode> Items { get; set; }

        public LENode(object item)
        {
            Item = item;
            Items = new ObservableCollection<LENode>();
        }
    }
}

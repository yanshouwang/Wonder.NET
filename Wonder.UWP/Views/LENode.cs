using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Views
{
    public class LENode
    {
        public object Value { get; set; }
        public ObservableCollection<LENode> Nodes { get; set; }

        public LENode(object value)
        {
            Value = value;
            Nodes = new ObservableCollection<LENode>();
        }
    }
}

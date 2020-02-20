using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Services
{
    public class ValueEventArgs : EventArgs
    {
        public byte[] Value { get; }

        public ValueEventArgs(byte[] value)
            : base()
        {
            Value = value;
        }
    }
}

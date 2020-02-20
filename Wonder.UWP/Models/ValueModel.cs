using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Models
{
    public class ValueModel
    {
        public int Type { get; set; }
        public byte[] Value { get; set; }
        public string ValueStr { get; set; }
        public DateTime Time { get; set; }

        public ValueModel(int type, byte[] value)
        {
            Type = type;
            Value = value;
            ValueStr = BitConverter.ToString(value).Replace("-", " ");
            Time = DateTime.Now;
        }
    }
}

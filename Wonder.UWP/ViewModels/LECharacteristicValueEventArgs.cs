using System;
using Windows.Storage.Streams;

namespace Wonder.UWP.ViewModels
{
    public class LECharacteristicValueEventArgs : EventArgs
    {
        public IBuffer Value { get; }

        public LECharacteristicValueEventArgs(IBuffer value)
        {
            Value = value;
        }
    }
}

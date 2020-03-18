using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Wonder.UWP.Logger
{
    internal static class Extensions
    {
        public static CreationCollisionOption ToCreationCollisionOption(this LogMode mode)
        {
            var option = mode == LogMode.All
                       ? CreationCollisionOption.OpenIfExists
                       : CreationCollisionOption.ReplaceExisting;
            return option;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Wonder.UWP.Xaml;

namespace Wonder.UWP.Services
{
    public interface IThemeService
    {
        void SetThemeMode(ThemeMode mode);
        void SetThemeColor(Color color);
    }
}

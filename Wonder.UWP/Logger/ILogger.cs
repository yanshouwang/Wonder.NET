using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public interface ILogger
    {
        Task LogAsync(string message, LogMode mode = LogMode.All);
    }
}

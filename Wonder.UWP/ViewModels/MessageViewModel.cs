using System;

namespace Wonder.UWP.ViewModels
{
    public class MessageViewModel
    {
        public MessageViewModel(string message)
        {
            Time = DateTime.Now;
            Message = message;
        }

        public DateTime Time { get; }
        public string Message { get; }
    }
}

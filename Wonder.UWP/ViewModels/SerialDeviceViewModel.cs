using Prism.Mvvm;

namespace Wonder.UWP.ViewModels
{
    public class SerialDeviceViewModel : BindableBase
    {
        public string PortName { get; }
        public string ID { get; }
        public string Name { get; }

        public SerialDeviceViewModel(string portName, string id, string name)
        {
            PortName = portName;
            ID = id;
            Name = name;
        }
    }
}

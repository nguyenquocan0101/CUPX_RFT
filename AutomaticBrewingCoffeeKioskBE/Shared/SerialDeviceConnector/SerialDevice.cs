
namespace SerialDeviceConnector
{
    public abstract class SerialDevice
    {
        readonly SerialPortManager _serialPortManager;
        protected string PortName { get; set; }
        protected int BaudRate { get; set; }
        protected SerialDevice(string portName, int baudRate)
        {
            PortName = portName;
            BaudRate = baudRate;
            _serialPortManager = new SerialPortManager(portName, baudRate);
        }

        public void Connect()
        {
            _serialPortManager.Open();
        }

        public void Disconnect()
        {
            _serialPortManager.Close();
        }

        protected byte[] SendCommand(byte[] command)
        {
            _serialPortManager.Write(command);
            Task.Delay(1000).Wait(); // Wait for the device to process the command
            byte[] response = _serialPortManager.Read();
            return response;
            //return ReceiveData(response).Data;
        }

        protected ParsedResponse ReceiveData(byte[] response)
        {
            return SerialResponseParser.Parse(response);
        }
        
        public string GetPortName()
        {
            return PortName;
        }
    }
}

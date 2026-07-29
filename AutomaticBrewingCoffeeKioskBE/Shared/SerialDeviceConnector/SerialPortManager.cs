using System.IO.Ports;

namespace SerialDeviceConnector
{
    public class SerialPortManager
    {
        private SerialPort _serialPort;
        public SerialPortManager(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        }

        public void Open() => _serialPort.Open();
        public void Close() => _serialPort.Close();

        public void Write(byte[] data) => _serialPort.Write(data, 0, data.Length);

        public byte[] Read()
        {
            byte[] buffer = new byte[_serialPort.BytesToRead]; 
            _serialPort.Read(buffer, 0, buffer.Length);

            return buffer;
        }
    }
}


using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace MixMachineController
{
    public class PinActivator
    {
        private readonly SerialPort serialPort;
        private readonly List<double> waitTimes;
        public PinActivator(string portName, int baudRate)
        {
            serialPort = new SerialPort
            {
                PortName = portName,          
                BaudRate = baudRate,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Encoding = Encoding.UTF8, //DO NOT REMOVE
                WriteTimeout = 1000,
                NewLine = "\n"
            };
        }

        public void Connect()
        {
            serialPort.Open();
        }

        public void DisConnect()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("Serial port closed.");
            }
        }

        public async Task<bool> RunOnTime(string command)
        {
            try
            {
                string responsePattern = "1";

                // Chuyển command thành số và làm tròn lên
                if (!double.TryParse(command, out double parsedValue))
                    throw new ArgumentException("Invalid numeric command");

                int roundedValue = (int)Math.Ceiling(parsedValue);
                Console.WriteLine("May hoat dong trong {0} s", roundedValue);
                serialPort.WriteLine(roundedValue.ToString());
                int safeTimeOut = 150;
                await Task.Delay(roundedValue * 1000 + safeTimeOut); //to second
                var response = serialPort.ReadLine();
                Console.WriteLine(response);
                var result = Regex.IsMatch(response, Regex.Escape(responsePattern));
                Console.WriteLine(result);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] RunOnTime failed: {e.Message}");
                return false;
            }
        }
    }
}

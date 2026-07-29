using System.IO.Ports;
using System.Text;

namespace InhaleController
{
    public class PinActivator
    {
        private readonly SerialPort serialPort;
        private readonly List<double> pumpTimeList;
        public PinActivator(string portName, int baudRate, List<double> pumpTimeList)
        {
            serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Encoding = Encoding.UTF8, //DO NOT REMOVE
                //ReadTimeout = 2000,         
                WriteTimeout = 1000,
                NewLine = "\n"
            };
            this.pumpTimeList = pumpTimeList;
        }

        public void Connect()
        {
            serialPort.Open();
        }

        public void DisConnect()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }

        public async Task<bool> RunAll(string command)
        {
            string responsePattern = "9"; // Kết quả mong đợi từ lệnh runAll  

            //string commandP = JsonConvert.DeserializeObject<string>(command); // kết quả là "#"

            string commandP = command;
            serialPort.WriteLine(commandP);
            await Task.Delay(150);
            var response = serialPort.ReadExisting();
            return response.Contains(responsePattern);
        }

        public async Task<bool> StopAll(string command)
        {
            string responsePattern = "0";
            serialPort.WriteLine(command);
            await Task.Delay(150);
            string response = serialPort.ReadExisting();
            return response.Contains(responsePattern);
        }

        public async Task<bool> RunTarget(string command)
        {
            try
            {
                string responsePattern = "1";

                double TIME_PER_100ML = pumpTimeList[0];
                string commandP = command;
                string[] commandPart = commandP.Split('|');

                int maxMl = 0;
                commandPart = commandPart.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();

                int commandPartLength = commandPart.Length;
                for (int i = 0; i < commandPartLength; i++)
                {
                    var part = commandPart[i].Trim();

                    if (string.IsNullOrWhiteSpace(part)) continue;

                    var split = part.Split('-');
                    if (split.Length != 2) continue;

                    if (int.TryParse(split[1], out int ml))
                    {
                        int pumpIndex = int.Parse(split[0]) - 1;
                        if (pumpIndex < 0 || pumpIndex >= pumpTimeList.Count)
                        {
                            Console.WriteLine($"[ERROR] Invalid pump index: {pumpIndex + 1}");
                            continue;
                        }
                        double pumpTime = pumpTimeList[pumpIndex];
                        if (pumpTime > TIME_PER_100ML)
                        {
                            TIME_PER_100ML = pumpTime;
                        }
                        if (ml > maxMl) maxMl = ml;
                    }
                }

                foreach (var part in commandPart)
                {
                    if (string.IsNullOrWhiteSpace(part)) continue;

                    var split = part.Split('-');
                    if (split.Length != 2) continue;

                    if (int.TryParse(split[1], out int ml))
                    {
                        if (ml > maxMl) maxMl = ml;
                    }
                }

                int delayMs = (int)((maxMl / 100.0) * TIME_PER_100ML * 1000);
                int safeDelayTime = 150;
                delayMs += safeDelayTime;

                serialPort.WriteLine(commandP);
                Console.WriteLine($"Doi {delayMs} ms de may bom hoan tat...");
                await Task.Delay(delayMs + 300); // thêm đệm để thiết bị phản hồi đầy đủ

                string response = string.Empty;
                string parternAfterRun = string.Empty;

                for (int i = 0; i < commandPartLength; i++)
                {
                    parternAfterRun += responsePattern;
                    await Task.Delay(150); // chờ từng relay phản hồi
                }

                // Đọc toàn bộ buffer còn lại
                byte[] rawBytes = new byte[serialPort.BytesToRead];
                serialPort.Read(rawBytes, 0, rawBytes.Length);
                response += Encoding.UTF8.GetString(rawBytes);

                Console.WriteLine($"Expected: \n{parternAfterRun}");
                Console.WriteLine($"Actual: \n{response}");

                bool result = response.Contains(parternAfterRun);
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
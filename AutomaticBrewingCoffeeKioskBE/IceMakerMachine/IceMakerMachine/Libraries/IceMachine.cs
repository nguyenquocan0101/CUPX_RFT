using SerialDeviceConnector;
using static IceMakerDevice.Libraries.IceMakerParameterCommand;
using static IceMakerDevice.Libraries.IceMakerStatusCommand;


namespace IceMakerDevice.Libraries
{

    public class IceMachine : SerialDevice
    {
        private IceMakerStatusCommand StatusCommand { get; set; }
        private IceMakerParameterCommand ParameterCommand { get; set; }
        private IceMakerPowerOffCommand PowerOffCommand { get; set; }
        private IceMakerDispenseCommand DispenseCommand { get; set; }

        public IceMachine(string portName, int baudRate) : base(portName, baudRate)
        {
            // Initialize commands
            StatusCommand = new IceMakerStatusCommand();
            ParameterCommand = new IceMakerParameterCommand();
            PowerOffCommand = new IceMakerPowerOffCommand();
            DispenseCommand = new IceMakerDispenseCommand();
        }

        public static Dictionary<string, string> GetLabels()
        {
            var iceMachineStatusLabels = new Dictionary<string, string>
            {
                { "IsIceBinFull", "Thùng đá đầy" },
                { "IsCondenserFault", "Lỗi dàn ngưng" },
                { "IsEvaporatorFault", "Lỗi dàn bay hơi" },
                { "IsWaterTankLow", "Mực nước thấp" },
                { "IsIceSystemFault", "Lỗi hệ thống làm đá" },
                { "IsWasteWaterTrayFull", "Khay nước thải đầy" },
                { "IsWaterInletFault", "Lỗi đầu vào nước" },
                { "IsWasteWaterTrayMissing", "Khay nước thải bị thiếu" },
                { "CurrentSystemStatus", "Trạng thái hệ thống hiện tại" },
                { nameof(IceMakerWorkingStatus.Standby), "Chế độ chờ" },
                { nameof(IceMakerWorkingStatus.MakingBeverage), "Đang pha chế đá" },
                { nameof(IceMakerWorkingStatus.Shutdown), "Đã tắt" },
                { nameof(IceMakerWorkingStatus.DispensingComplete), "Hoàn tất thả đá" },
                { nameof(IceMakerWorkingStatus.Cancelled), "Đã hủy (quay về chế độ chờ)" },
                { nameof(IceMakerWorkingStatus.BootInitialization), "Đang khởi động" },
                { nameof(IceMakerWorkingStatus.FaultState), "Trạng thái lỗi" },
                { nameof(IceMakerWorkingStatus.Unknown), "Không xác định" },
            };
            return iceMachineStatusLabels;

        }

        // 1. Slave Status Query (0x01)
        public IceMakerStatusCommand QueryStatus()
        {
            byte[] command = StatusCommand.GetQueryStatusCommand();

            // Send command and receive response
            var responseData = SendCommand(command);
            StatusCommand.HandleResponse(responseData);
            return StatusCommand;
        }

        // 2. Query Parameters (0x02)
        public IceMakerParameterCommand QueryParameters()
        {
            byte[] command = ParameterCommand.GetQueryParametersCommand();

            // Send command and receive response
            var responseData = SendCommand(command);
            ParameterCommand.HandleQueryResponse(responseData);
            return ParameterCommand;
        }

        // 2.1 Set Parameters (0x02)
        // Trong IceMachine, hãy đổi kiểu dữ liệu của tham số để nhận double
        public bool SetParameters(string language, double iceQty, double waterQty, double iceWaterQty)
        {
            var enumLanguage = Enum.Parse<IceMakerLanguage>(language);

            byte iceQtyByte = Convert.ToByte(Math.Round(iceQty));
            byte waterQtyByte = Convert.ToByte(Math.Round(waterQty));
            byte iceWaterQtyByte = Convert.ToByte(Math.Round(iceWaterQty));

            byte[] command = ParameterCommand.SetParametersCommand(enumLanguage, iceQtyByte, waterQtyByte, iceWaterQtyByte);
            // Send command and receive response
            var responseData = SendCommand(command);
            return ParameterCommand.HandleSetResponse(responseData);
        }

        // 3. Execute
        public IceMakerDispenseCommand Excecute(byte type, byte quantity)
        {
            byte[] command = DispenseCommand.GetDispenseCommand(type, quantity);
            var responseData = SendCommand(command);
            Console.WriteLine("Command: " + BitConverter.ToString(command));
            Console.WriteLine("Response Length: " + responseData.Length);
            Console.WriteLine("Response: " + BitConverter.ToString(responseData));
            DispenseCommand.HandleResponse(responseData);
            return DispenseCommand;
        }

        // 4. Power Off (0x03)
        public IceMakerPowerOffCommand PowerOff()
        {
            byte[] command = PowerOffCommand.GetPowerOffCommand();
            var responseData = SendCommand(command);
            PowerOffCommand.HandleResponse(responseData);
            return PowerOffCommand;
        }

        //public IceMakerDispenseCommand DispenseIce(byte quantity)
        //{
        //    byte[] command = DispenseCommand.GetDispenseCommand(IceMakerDispenseCommand.IceMakerBeverageType.Ice, quantity);
        //    var responseData = SendCommand(command);
        //    DispenseCommand.HandleResponse(responseData);
        //    return DispenseCommand;
        //}

        //public IceMakerDispenseCommand DispenseWater(byte quantity)
        //{
        //    byte[] command = DispenseCommand.GetDispenseCommand(IceMakerDispenseCommand.IceMakerBeverageType.Water, quantity);
        //    var responseData = SendCommand(command);
        //    DispenseCommand.HandleResponse(responseData);
        //    return DispenseCommand;
        //}

        //public IceMakerDispenseCommand DispenseIceWater(byte quantity)
        //{
        //    byte[] command = DispenseCommand.GetDispenseCommand(IceMakerDispenseCommand.IceMakerBeverageType.IceWater, quantity);
        //    var responseData = SendCommand(command);
        //    DispenseCommand.HandleResponse(responseData);
        //    return DispenseCommand;
        //}
    }
}
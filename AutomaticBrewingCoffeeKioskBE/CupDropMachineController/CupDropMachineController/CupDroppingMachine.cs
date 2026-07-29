using SerialDeviceConnector;

namespace CupDropMachineController
{
    public class CupDroppingMachine : SerialDevice
    {
        private SlaveStatusCommand SlaveStatusCommand { get; set; }
        private ShutdownCommand ShutdownCommand { get; set; }
        private DispenseBeverageCommand DispenseBeverageCommand { get; set; }

        public CupDroppingMachine(string portName, int baudRate) : base(portName, baudRate)
        {
            SlaveStatusCommand = new SlaveStatusCommand();
            DispenseBeverageCommand = new DispenseBeverageCommand();
            ShutdownCommand = new ShutdownCommand();
        }

        public static Dictionary<string, string> GetLabels()
        {
            var deviceStatusLabels = new Dictionary<string, string>
            {
                { "DeviceId", "Mã định danh thiết bị" },
                { "IsNoCup", "Không có cốc" },
                { "IsCupNotTakenAway", "Cốc chưa được lấy đi" },
                { "IsDrawerPulledOut", "Ngăn kéo bị kéo ra" },
                { "IsMotorFailure", "Lỗi động cơ" },
                { "IsRobotArmInPlace", "Tay robot đúng vị trí" },
                { "CurrentSystemStatus", "Trạng thái hệ thống hiện tại" },
                { SystemStatus.Standby.ToString(), "Chờ sẵn" },
                { SystemStatus.CupDroppingInProgress.ToString(), "Đang thả cốc" },
                { SystemStatus.HasFault.ToString(), "Có lỗi" },
                { SystemStatus.Unknown.ToString(), "Không xác định" }
            };
            return deviceStatusLabels;
        }

        // 1. Slave Status Query (0x01)
        public SlaveStatusCommand QueryStatus()
        {
            byte[] command = SlaveStatusCommand.GetCommand();

            // Send command and receive response
            var responseData = SendCommand(command);
            SlaveStatusCommand.HandleResponse(responseData);
            return SlaveStatusCommand;
        }

        // 3. Shutdown (0x03)k
        // Not support yet
        public ShutdownCommand Shutdown()
        {
            byte[] command = ShutdownCommand.GetCommand();

            // Send command and receive response
            var responseData = SendCommand(command);
            ShutdownCommand.HandleResponse(responseData);
            return ShutdownCommand;
        }

        public DispenseBeverageCommand DropOneCup()
        {
            byte[] command = DispenseBeverageCommand.GetCommand();
            var responseData = SendCommand(command);
            DispenseBeverageCommand.HandleResponse(responseData);
            return DispenseBeverageCommand;
        }
    }
}

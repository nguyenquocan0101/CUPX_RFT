using SerialDeviceConnector;
using static CoffeeMachineController.EventHandlerCommand;

namespace CoffeeMachineController
{
    public class CoffeeMachine : SerialDevice
    {
        private SlaveStatusCommand _statusCommand { get; set; }
        private ShutdownCommand _shutdownCommand { get; set; }
        private DrinkOrCleanCommand _drinkOrCleanCommand { get; set; }
        private EventHandlerCommand _eventHandlerCommand { get; set; }

        public CoffeeMachine(string portName, int baudRate) : base(portName, baudRate)
        {
            _statusCommand = new SlaveStatusCommand();
            _shutdownCommand = new ShutdownCommand();
            _drinkOrCleanCommand = new DrinkOrCleanCommand();
            _eventHandlerCommand = new EventHandlerCommand();
        }

        public static Dictionary<string, string> GetLabels()
        {
            var labels = new Dictionary<string, string>()
            {
                { "HasFault", "Có lỗi" },

                // Hệ thống
                { "CurrentSystemStatus", "Trạng thái hệ thống hiện tại" },
                { "Initialization", "Đang khởi tạo" },
                { "Idle", "Đang chờ" },
                { "Running", "Đang hoạt động" },
                { "Shutdown", "Đang tắt" },
                { "Unknown", "Không xác định" },

                { "IsCoffeeBoilerDisconnected", "Nồi hơi cà phê bị ngắt kết nối" },
                { "IsSteamBoilerDisconnected", "Nồi hơi hơi nước bị ngắt kết nối" },
                { "IsCoffeeBoilerNtcFault", "Lỗi cảm biến nhiệt nồi hơi cà phê" },
                { "IsSteamBoilerNtcFault", "Lỗi cảm biến nhiệt nồi hơi hơi nước" },
                { "IsCoffeeBoilerTempTooLow", "Nhiệt độ nồi hơi cà phê quá thấp" },
                { "IsSteamBoilerTempTooLow", "Nhiệt độ nồi hơi hơi nước quá thấp" },
                { "IsCoffeeBoilerTempTooHigh", "Nhiệt độ nồi hơi cà phê quá cao" },
                { "IsSteamBoilerTempTooHigh", "Nhiệt độ nồi hơi hơi nước quá cao" },
                { "IsCoffeePipeBlocked", "Đường ống cà phê bị tắc" },
                { "IsNormalTempWaterPipeBlocked", "Đường ống nước thường bị tắc" },
                { "IsGrinder1SystemAbnormal", "Hệ thống xay 1 bất thường" },
                { "IsGrinder2SystemAbnormal", "Hệ thống xay 2 bất thường" },
                { "IsBean1Empty", "Hết hạt cà phê 1" },
                { "IsBean2Empty", "Hết hạt cà phê 2" },
                { "IsDeliveryPort1SwitchAbnormal", "Công tắc cổng lấy đồ uống 1 bất thường" },
                { "IsDeliveryPort2SwitchAbnormal", "Công tắc cổng lấy đồ uống 2 bất thường" },
                { "IsIngredient1Empty", "Hết nguyên liệu 1" },
                { "IsIngredient2Empty", "Hết nguyên liệu 2" },
                { "IsWaterInletPressureAbnormal", "Áp lực nước đầu vào bất thường" },
                { "IsBrewDoorOpen", "Cửa pha cà phê đang mở" },
                { "IsBrewerNotInstalled", "Bộ pha cà phê chưa lắp đặt" },
                { "IsMilkChannel3Empty", "Hết sữa kênh 3" },
                { "IsMilkChannel1Empty", "Hết sữa kênh 1" },
                { "IsWaterStoragePanNeeded", "Cần khay chứa nước" },
                { "IsDripTrayFull", "Khay hứng nước đầy" },
                { "IsWasteBinFull", "Thùng rác đầy" },
                { "IsBrewerDeviceFailure", "Lỗi thiết bị pha cà phê" },
                { "IsBrewingPressureTooHigh", "Áp suất pha cà phê quá cao" },
                { "IsBean3Empty", "Hết hạt cà phê 3" },
                { "IsStirrerNotInstalled", "Chưa lắp cánh khuấy" },
                { "IsInstantBoilerDisconnected", "Nồi hơi tức thời bị ngắt kết nối" },
                { "IsInstantBoilerNtcFault", "Lỗi cảm biến nhiệt nồi hơi tức thời" },
                { "IsInstantBoilerTempTooLow", "Nhiệt độ nồi hơi tức thời quá thấp" },
                { "IsInstantBoilerTempTooHigh", "Nhiệt độ nồi hơi tức thời quá cao" },
                { "IsDeliveryPortFailure", "Lỗi cổng lấy đồ uống" },
                { "IsMilkChannel2Empty", "Hết sữa kênh 2" },
            };
            for (int i = 1; i <= 96; i++)
            {
                labels.Add($"IsDrinkId{i}Unavailable", $"Đồ uống số {i} không khả dụng");
            }

            labels.Add("IsBrewerMotorOverheated", "Động cơ pha cà phê quá nhiệt");
            labels.Add("IsPipelineLeakage", "Rò rỉ đường ống");
            labels.Add("IsBeanBox1PhotoelectricLow", "Hộp hạt 1 mức quang điện thấp");
            labels.Add("IsBeanBox2PhotoelectricLow", "Hộp hạt 2 mức quang điện thấp");
            labels.Add("IsBeanBox3PhotoelectricLow", "Hộp hạt 3 mức quang điện thấp");
            labels.Add("IsBeanBox1NotInstalled", "Hộp hạt 1 chưa lắp đặt");
            labels.Add("IsBeanBox2NotInstalled", "Hộp hạt 2 chưa lắp đặt");
            labels.Add("IsBeanBox3NotInstalled", "Hộp hạt 3 chưa lắp đặt");
            labels.Add("IsBeanBinHandleNotInPlace", "Tay cầm hộp hạt chưa đúng vị trí");
            labels.Add("IsPowderMixingBinNotInstalled", "Thùng trộn bột chưa lắp đặt");
            labels.Add("IsPhotoelectricModuleFailure", "Lỗi mô-đun quang điện");

            return labels;
        }

        // 1. Slave Status Query (0x01)
        public SlaveStatusCommand QueryStatus()
        {
            byte[] command = _statusCommand.GetQueryStatusCommand();
            //Console.WriteLine(BitConverter.ToString(command));
            var responseData = SendCommand(command);
            _statusCommand.HandleSResponse(responseData);
            return _statusCommand;
        }

        // 3. Shutdown (0x03)
        public ShutdownCommand Shutdown()
        {
            byte[] command = _shutdownCommand.GetShutDownCommand();
            //Console.WriteLine(BitConverter.ToString(command));
            var responseData = SendCommand(command);
            _statusCommand.HandleSResponse(responseData);
            return _shutdownCommand;
        }

        // 4. Drinks (0x04)
        public DrinkOrCleanCommand MakeDrink(int drinkNumber)
        {
            // Validate Drink Number (1-100)
            if (drinkNumber < 1 || drinkNumber > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(drinkNumber),
                    "Drink number must be between 1 and 100.");
            }

            var drinkNo = ParseIntToByte(drinkNumber);
            byte[] command = _drinkOrCleanCommand.GetMakeDrinkCommand((byte)DrinkOrCleanCommand.CommandAction.DispenseDrink,
                    drinkNo);
            //Console.WriteLine(BitConverter.ToString(command));
            // Send command and receive response
            var responseData = SendCommand(command);
            //Console.WriteLine("Command: " + BitConverter.ToString(command));
            //Console.WriteLine("Response Length: " + responseData.Length);
            //Console.WriteLine("Response: " + BitConverter.ToString(responseData));

            _drinkOrCleanCommand.HandleResponseCommand(responseData);
            return _drinkOrCleanCommand;
        }


        private byte ParseIntToByte(int value)
        {
            if (value < byte.MinValue && value > byte.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
            }

            byte myByte = (byte)value;
            return myByte;
        }

        //4. Clean (0x04)
        public DrinkOrCleanCommand Clean(DrinkOrCleanCommand.CommandAction action)
        {
            if (action == DrinkOrCleanCommand.CommandAction.DispenseDrink)
            {
                throw new InvalidOperationException($"'{action}' is not a cleaning action.");
            }

            byte[] command = _drinkOrCleanCommand.GetCleanCommand((byte)action);
            //Console.WriteLine(BitConverter.ToString(command));
            var responseData = SendCommand(command);
            _drinkOrCleanCommand.HandleResponseCommand(responseData);
            return _drinkOrCleanCommand;
        }

        //6. Event Handler
        public EventHandlerCommand HandleEvent(EventCommandEnum eventCommand, TargetEvent target)
        {
            _eventHandlerCommand.EventCommand = eventCommand;
            _eventHandlerCommand.Target = target;

            byte[] command = _eventHandlerCommand.GetEventHandlerCommand((byte)eventCommand, (byte)target);
            //Console.WriteLine(BitConverter.ToString(command));
            var responseData = SendCommand(command);
            _eventHandlerCommand.HandleSResponse(responseData);
            return _eventHandlerCommand;
        }
    }

}
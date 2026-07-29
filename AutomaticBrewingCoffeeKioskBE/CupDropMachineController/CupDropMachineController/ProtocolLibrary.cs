
using SerialDeviceConnector;

namespace CupDropMachineController
{
   

    /// <summary>
    /// "reserve" -> NOT DEFINE YET
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class CDMCode
    {
        #region Command Codes (Byte 1)

        /// <summary> Command: Query slave status (Host->Slave & Slave->Host) </summary>
        public const byte Cmd_StatusQuery = 0x01;

        /// <summary> Command: Query or set slave parameters (Host->Slave & Slave->Host) </summary>
        public const byte Cmd_ParameterQuerySet = 0x02;

        /// <summary> Command: Request slave shutdown (Host->Slave) </summary>
        public const byte Cmd_Shutdown = 0x03;

        /// <summary> Command: Drop cup action (Host->Slave) </summary>
        public const byte Cmd_DispenseBeverage = 0x04;

        #endregion

        #region Instruction Codes (Byte 3)

        /// <summary> Instruction: Indicates a query operation. </summary>
        public const byte Inst_Query = 0x55;

        /// <summary> Instruction: Indicates a set, action, or command execution. </summary>
        public const byte Inst_SetOrAction = 0xAA;

        #endregion
    }

    #region 0x01 Slave status query

    public class SlaveStatusCommand
    {
        private readonly CommandBuilder _commandBuilder;
        public byte CommandCode { get; set; }
        public byte LengthCode { get; set; }
        public byte InstructionCode { get; set; }
        public Data1Bit Data1 { get; set; } = default!;
        public Data2Bit Data2 { get; set; } = default!;
        public byte CheckCode { get; set; }
        public byte EndCode { get; set; }

        public SlaveStatusCommand()
        {
            _commandBuilder = new CommandBuilder();
        }

        public byte[] GetCommand()
        {
            //(host → slave）
            //Send Command: (Command Code)0x01 | Lengthcode | InstructionCode(0x55)| Check code | End code
            return _commandBuilder
                .AddCommandCode(CDMCode.Cmd_StatusQuery)
                .AddInstructionCode(CDMCode.Inst_Query)
                .Build();
        }

        public void HandleResponse(byte[] responseData)
        {
            //(host←slave)
            //Receive Command: (Command Code)0x01 |	Lengthcode | 0x55 |	Data 1	Data 2	Data 3	Data 4....Data 19 |	Check code | End code
            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];
            Data1 = new Data1Bit(responseData[3]);
            Data2 = new Data2Bit(responseData[4]);
            CheckCode = responseData[5];
            EndCode = responseData[6];
        }

        public class Data1Bit : DataBit
        {
            public Data1Bit(byte data)
            {
                LoadFromByte(data);
            }

            public bool IsNoCup => Bits[0];
            public bool IsCupNotTakenAway => Bits[1];
            public bool IsDrawerPulledOut => Bits[2];
            public bool IsMotorFailure => Bits[3];
            public bool IsRobotArmInPlace => Bits[4];
        }

        public class Data2Bit : DataBit
        {
            public Data2Bit(byte data)
            {
                LoadFromByte(data);
                CurrentSystemStatus = (SystemStatus)(data >> 1 & 0b11);
            }

            public SystemStatus CurrentSystemStatus { get; set; }
        }
    }

    public class DataBit
    {
        protected bool[] Bits = new bool[8];

        public void LoadFromByte(byte data)
        {
            for (int i = 0; i < 8; i++)
            {
                Bits[i] = (data & 1 << i) != 0;
            }
        }
    }

    #endregion

    #region 0x03 Shutdown

    //! Note: Shutdown can only be performed when the slave is in a non-fault state
    public class ShutdownCommand
    {
        private readonly CommandBuilder commandBuilder;

        public ShutdownCommand()
        {
            commandBuilder = new CommandBuilder();
        }

        public byte CommandCode { get; set; }
        public byte LengthCode { get; set; }
        public byte InstructionCode { get; set; }
        public byte CheckCode { get; set; }
        public byte EndCode { get; set; }

        //--- Data 1 ---
        public OperationResult Result { get; set; }


        public byte[] GetCommand()
        {
            //(host → slave)
            //(Command Code)0x03 | Length code | (InstructionCode)0xAA | Check code | End code
            return commandBuilder
                .AddCommandCode(CDMCode.Cmd_Shutdown)
                .AddInstructionCode(CDMCode.Inst_SetOrAction)
                .Build();
        }

        public void HandleResponse(byte[] responseData)
        {
            //(host←slave)
            //(Command Code)0x03 | Length code | (InstructionCode)0xAA | Data1 | Check code | End code
            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];
            Result = Enum.TryParse<OperationResult>(responseData[3].ToString(), out var result) ? result : default;
            CheckCode = responseData[4];
            EndCode = responseData[5];
        }
    }

    #endregion

    #region 0x04 Dispense Beverage

    public class DispenseBeverageCommand
    {
        private readonly CommandBuilder _commandBuilder;
        public byte CommandCode { get; set; }
        public byte LengthCode { get; set; }
        public byte InstructionCode { get; set; }
        private byte BeverageNumber { get; set; }
        public byte CheckCode { get; set; }
        public byte EndCode { get; set; }

        public DispenseBeverageCommand()
        {
            _commandBuilder = new CommandBuilder();
        }

        //--- Data 1 ---
        public OperationResult Result { get; set; }

        /// <summary>
        /// This function get the command with only 1 cup and data1 is 0x00 for reserved
        /// </summary>
        /// <returns></returns>
        public byte[] GetCommand()
        {
            return _commandBuilder
                .AddCommandCode(CDMCode.Cmd_DispenseBeverage)
                .AddInstructionCode(CDMCode.Inst_SetOrAction)
                .AddData(0x01, 0x00)
                .Build();
        }

        public void HandleResponse(byte[] responseData)
        {
            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];
            BeverageNumber = responseData[3];
            Result = Enum.TryParse<OperationResult>(responseData[4].ToString(), out var result)
                ? result
                : default;
            CheckCode = responseData[4];
            EndCode = responseData[5];
        }
    }

    #endregion

    // ReSharper disable once InconsistentNaming
    public enum SystemStatus
    {
        Standby = 0b00,
        CupDroppingInProgress = 0b01,
        HasFault = 0b10,
        Unknown = -1
    }

    public enum OperationResult : byte
    {
        /// <summary>The requested operation failed.</summary>
        Failed = 0x00,

        /// <summary>The requested operation succeeded.</summary>
        Success = 0x01
    }
}

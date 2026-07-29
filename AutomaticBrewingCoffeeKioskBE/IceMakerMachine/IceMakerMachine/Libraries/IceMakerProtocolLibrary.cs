using System.Buffers.Binary; // For potential Endian handling if needed
using System.Text;
using IceMakerDevice.Libraries;
using SerialDeviceConnector;

namespace IceMakerDevice.Libraries
{
    /// <summary>
    /// Defines constants for the Z01/Z02/Z03 Ice Maker Serial Communication Protocol V0.0.3.
    /// </summary>
    public static class IceMakerCMCode
    {
        #region Command Codes (Byte 1)

        /// <summary> Command: Query slave status (0x01) </summary>
        public const byte Cmd_StatusQuery = 0x01;

        /// <summary> Command: Query or Set slave parameters (0x02) </summary>
        public const byte Cmd_ParameterQuerySet = 0x02;

        /// <summary> Command: Request slave power off (Z03 Only) (0x03) </summary>
        public const byte Cmd_PowerOff = 0x03;

        /// <summary> Command: Dispense Beverage (0x04) </summary>
        public const byte Cmd_DispenseBeverage = 0x04;

        #endregion

        #region Instruction Codes (Byte 3)

        /// <summary> Instruction: Indicates a query operation (0x55). </summary>
        public const byte Inst_Query = 0x55;

        /// <summary> Instruction: Indicates a set, action, or command execution (0xAA). </summary>
        public const byte Inst_SetOrAction = 0xAA;

        #endregion

        #region Common Values

        /// <summary> End code for all packets (0xFF). </summary>
        public const byte EndCodeValue = 0xFF;

        #endregion
    }

    /// <summary>
    /// Indicates whether a Set/Action command succeeded or failed (used in responses).
    /// </summary>
    public enum OperationResult : byte
    {
        /// <summary>The requested operation failed (0x00).</summary>
        Failed = 0x00,

        /// <summary>The requested operation succeeded (0x01).</summary>
        Success = 0x01
    }

    // ==============================================================
    // Base class for Bitmask Data Bytes (inspired by sample)
    // ==============================================================
    public abstract class DataBitFlags
    {
        protected byte RawValue { get; private set; }
        protected bool[] Bits = new bool[8];

        protected void LoadFromByte(byte data)
        {
            RawValue = data;
            for (int i = 0; i < 8; i++)
            {
                Bits[i] = (data & 1 << i) != 0;
            }
        }

        public override string ToString()
        {
            return $"0x{RawValue:X2}";
        }
    }

    // ==============================================================
    // Command 0x01: Query Slave Status
    // ==============================================================

    /// <summary>
    /// Handles Command 0x01: Query Slave Status.
    /// </summary>
    public class IceMakerStatusCommand
    {
        private readonly CommandBuilder commandBuilder;

        // Response Properties
        public byte CommandCode { get; private set; }
        public byte LengthCode { get; private set; }
        public byte InstructionCode { get; private set; }
        public StatusData1Flags? Data1_FaultStatus { get; private set; }
        public IceMakerWorkingStatus Data2_WorkingStatus { get; private set; }
        public StatusData3Flags? Data3_AdditionalStatus_Motong { get; private set; } // Null if not Motong version
        public byte CheckCode { get; private set; }
        public byte EndCode { get; private set; }
        public bool IsMotongResponse { get; private set; }

        public IceMakerStatusCommand()
        {
            // Assuming CommandBuilder is available via DI or service locator,
            // or instantiate it directly if appropriate.
            commandBuilder = new CommandBuilder();
        }

        /// <summary>
        /// Gets the byte array for a Status Query request.
        /// Structure: [0x01] [0x05] [0x55] [Checksum] [0xFF]
        /// </summary>
        /// <returns>The command byte array.</returns>
        public byte[] GetQueryStatusCommand()
        {
            return commandBuilder
                .AddCommandCode(IceMakerCMCode.Cmd_StatusQuery)
                .AddInstructionCode(IceMakerCMCode.Inst_Query)
                // CommandBuilder should calculate Length (5) and Checksum automatically
                .Build();
        }

        /// <summary>
        /// Parses the response byte array for a Status Query.
        /// Structure: [0x01] [Length] [0x55] [Data1] [Data2] ([Data3]) [Checksum] [0xFF]
        /// Length is 7 (Standard) or 8 (Motong).
        /// </summary>
        /// <param name="responseData">The raw byte array received from the slave.</param>
        /// <returns>True if parsing was successful, false otherwise (e.g., wrong command code, length, end code).</returns>
        public bool HandleResponse(byte[] responseData)
        {
            if (responseData == null || responseData.Length < 7) return false;
            if (responseData[0] != IceMakerCMCode.Cmd_StatusQuery) return false;
            if (responseData[2] != IceMakerCMCode.Inst_Query) return false;
            if (responseData[responseData.Length - 1] != IceMakerCMCode.EndCodeValue) return false;

            byte expectedLength = responseData[1];
            if (responseData.Length != expectedLength) return false;
            // Basic checksum validation could be added here if CommandBuilder doesn't do it

            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];

            Data1_FaultStatus = new StatusData1Flags(responseData[3]);
            //Console.WriteLine($"Data1_FaultStatus: {Data1_FaultStatus}"); // Debug log
            Data2_WorkingStatus = Enum.IsDefined(typeof(IceMakerWorkingStatus), responseData[4])
                ? (IceMakerWorkingStatus)responseData[4]
                : IceMakerWorkingStatus.Unknown;
            //Console.WriteLine($"Data2_WorkingStatus: {Data2_WorkingStatus}"); // Debug log
            IsMotongResponse = LengthCode == 8;
            if (IsMotongResponse && responseData.Length >= 8)
            {
                Data3_AdditionalStatus_Motong = new StatusData3Flags(responseData[5]);
                CheckCode = responseData[6];
                EndCode = responseData[7];
            }
            else if (!IsMotongResponse && responseData.Length == 7)
            {
                Data3_AdditionalStatus_Motong = null; // Ensure it's null for standard
                CheckCode = responseData[5];
                EndCode = responseData[6];
            }
            else
            {
                return false; // Length mismatch
            }

            return true;
        }

        /// <summary>
        /// Represents Data 1 in the Status Response (Fault & Status Flags).
        /// </summary>
        public class StatusData1Flags : DataBitFlags
        {
            public StatusData1Flags(byte data)
            {
                LoadFromByte(data);
            }

            /// <summary>Bit 0: 1 = Ice bin full (normal operation), 0 = Not full</summary>
            public bool IsIceBinFull => Bits[0];

            /// <summary>Bit 1: 1 = Condenser fault, 0 = Normal</summary>
            public bool IsCondenserFault => Bits[1];

            /// <summary>Bit 2: 1 = Evaporator fault, 0 = Normal</summary>
            public bool IsEvaporatorFault => Bits[2];

            /// <summary>Bit 3: 1 = Water tank low, 0 = Normal</summary>
            public bool IsWaterTankLow => Bits[3];

            /// <summary>Bit 4: 1 = Ice making system abnormal (fault), 0 = Normal</summary>
            public bool IsIceSystemFault => Bits[4];

            /// <summary>Bit 5: 1 = Empty waste water tray required, 0 = Tray OK</summary>
            public bool IsWasteWaterTrayFull => Bits[5];

            /// <summary>Bit 6: 1 = System water inlet abnormal (fault), 0 = Normal</summary>
            public bool IsWaterInletFault => Bits[6];

            /// <summary>Bit 7: 1 = Install waste water tray required, 0 = Tray present</summary>
            public bool IsWasteWaterTrayMissing => Bits[7];
        }

        /// <summary>
        /// Represents Data 3 in the Status Response (Additional Status - Motong Customers Only).
        /// </summary>
        public class StatusData3Flags : DataBitFlags
        {
            public StatusData3Flags(byte data)
            {
                LoadFromByte(data);
            }

            /// <summary>Bit 0: 1 = Ice maker is filling water, 0 = Not filling</summary>
            public bool IsFillingWater => Bits[0];

            /// <summary>Bit 1: 1 = System short of ice (normal during initial fill), 0 = Ice available/full</summary>
            public bool IsShortOfIce => Bits[1];
            // Bits 2-7 are reserved
        }

        /// <summary>
        /// Represents Data 2 in the Status Response (System Working Status).
        /// </summary>
        public enum IceMakerWorkingStatus : byte
        {
            Standby = 0, // Standby (Ready for commands)
            MakingBeverage = 1, // Cooking / Making Beverage (Can accept quantity adjustments)
            Shutdown = 2, // Shutdown
            DispensingComplete = 3, // Please Enjoy / Dispensing Complete (Cannot adjust quantity)
            Cancelled = 4, // Cancelled (Returning to Standby)
            BootInitialization = 5, // Boot Initialization
            FaultState = 6, // Fault State
            Unknown = 0xFF // Placeholder for undefined values
        }
    }

    // ==============================================================
    // Command 0x02: Query/Set Slave Parameters
    // ==============================================================

    /// <summary>
    /// Handles Command 0x02: Query and Set Slave Parameters.
    /// </summary>
    public class IceMakerParameterCommand
    {
        private readonly CommandBuilder commandBuilder;

        // Query Response Properties
        public double CondenserTempCelsius { get; set; }
        public double EvaporatorTempCelsius { get; set; }
        public double AmbientTempCelsius { get; set; }
        public IceMakerLanguage CurrentLanguage { get; set; } = IceMakerLanguage.Unknown;
        public byte DefaultIceQuantity { get; set; }
        public byte DefaultWaterQuantity { get; set; }
        public byte DefaultIceWaterQuantity { get; set; }
        public string VersionNumber { get; set; } = string.Empty;

        // Set Response Property
        public OperationResult SetResult { get; private set; } = OperationResult.Failed;

        // Common Packet Info (populated by Handle methods)
        public byte CommandCode { get; private set; }
        public byte LengthCode { get; private set; }
        public byte InstructionCode { get; private set; }
        public byte CheckCode { get; private set; }
        public byte EndCode { get; private set; }

        public IceMakerParameterCommand()
        {
            commandBuilder = new CommandBuilder();
        }

        /// <summary>
        /// Gets the byte array for a Parameter Query request.
        /// Structure: [0x02] [0x05] [0x55] [Checksum] [0xFF]
        /// </summary>
        /// <returns>The command byte array.</returns>
        public byte[] GetQueryParametersCommand()
        {
            return commandBuilder
                .AddCommandCode(IceMakerCMCode.Cmd_ParameterQuerySet)
                .AddInstructionCode(IceMakerCMCode.Inst_Query)
                .Build(); // Length=5
        }

        /// <summary>
        /// Parses the response byte array for a Parameter Query.
        /// Structure: [0x02] [0x12] [0x55] [D1..D13] [Checksum] [0xFF] (Length=18)
        /// </summary>
        /// <param name="responseData">The raw byte array received from the slave.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool HandleQueryResponse(byte[] responseData)
        {
            const int expectedLength = 18;
            if (responseData == null || responseData.Length != expectedLength)
            {
                Console.WriteLine("Invalid response: Data is null or length is not 18.");
                return false;
            }

            if (responseData[0] != IceMakerCMCode.Cmd_ParameterQuerySet)
            {
                Console.WriteLine(
                    $"Invalid CommandCode: Expected {IceMakerCMCode.Cmd_ParameterQuerySet:X2}, got {responseData[0]:X2}.");
                return false;
            }

            if (responseData[1] != expectedLength)
            {
                Console.WriteLine($"Invalid LengthCode: Expected {expectedLength}, got {responseData[1]}.");
                return false;
            }

            if (responseData[2] != IceMakerCMCode.Inst_Query)
            {
                Console.WriteLine(
                    $"Invalid InstructionCode: Expected {IceMakerCMCode.Inst_Query:X2}, got {responseData[2]:X2}.");
                return false;
            }

            if (responseData[expectedLength - 1] != IceMakerCMCode.EndCodeValue)
            {
                Console.WriteLine(
                    $"Invalid EndCode: Expected {IceMakerCMCode.EndCodeValue:X2}, got {responseData[expectedLength - 1]:X2}.");
                return false;
            }

            // Assign values
            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];

            // Assuming Big Endian (MSB first) based on D1 D2 ordering in doc
            ushort condenserRaw = BinaryPrimitives.ReadUInt16BigEndian(responseData.AsSpan(3)); // D1-D2
            ushort evaporatorRaw = BinaryPrimitives.ReadUInt16BigEndian(responseData.AsSpan(5)); // D3-D4
            ushort ambientRaw = BinaryPrimitives.ReadUInt16BigEndian(responseData.AsSpan(7)); // D5-D6

            CondenserTempCelsius = condenserRaw / 10.0;
            EvaporatorTempCelsius = (evaporatorRaw - 500) / 10.0;
            AmbientTempCelsius = ambientRaw / 10.0;

            CurrentLanguage = Enum.IsDefined(typeof(IceMakerLanguage), responseData[9]) // D7
                ? (IceMakerLanguage)responseData[9]
                : IceMakerLanguage.Unknown;

            DefaultIceQuantity = responseData[10]; // D8
            DefaultWaterQuantity = responseData[11]; // D9
            DefaultIceWaterQuantity = responseData[12]; // D10

            // Version is 3 ASCII chars (D11, D12, D13)
            VersionNumber = Encoding.ASCII.GetString(responseData, 13, 3);

            CheckCode = responseData[expectedLength - 2];
            EndCode = responseData[expectedLength - 1];

            // Log the parsed response data
            Console.WriteLine("Response received and parsed successfully:");
            Console.WriteLine($"CommandCode: 0x{CommandCode:X2}");
            Console.WriteLine($"LengthCode: {LengthCode}");
            Console.WriteLine($"InstructionCode: 0x{InstructionCode:X2}");
            Console.WriteLine($"Condenser Temperature: {CondenserTempCelsius:F1}°C (Raw: {condenserRaw})");
            Console.WriteLine($"Evaporator Temperature: {EvaporatorTempCelsius:F1}°C (Raw: {evaporatorRaw})");
            Console.WriteLine($"Ambient Temperature: {AmbientTempCelsius:F1}°C (Raw: {ambientRaw})");
            Console.WriteLine($"Current Language: {CurrentLanguage}");
            Console.WriteLine($"Default Ice Quantity: {DefaultIceQuantity}");
            Console.WriteLine($"Default Water Quantity: {DefaultWaterQuantity}");
            Console.WriteLine($"Default Ice+Water Quantity: {DefaultIceWaterQuantity}");
            Console.WriteLine($"Version Number: {VersionNumber}");
            Console.WriteLine($"CheckCode: 0x{CheckCode:X2}");
            Console.WriteLine($"EndCode: 0x{EndCode:X2}");

            return true;
        }

        /// <summary>
        /// Gets the byte array for a Parameter Set request.
        /// Structure: [0x02] [0x0F] [0xAA] [D1..D6(Reserved)] [D7(Lang)] [D8(Ice)] [D9(Water)] [D10(IceWater)] [Checksum] [0xFF] (Length=15)
        /// </summary>
        /// <param name="language">The language to set.</param>
        /// <param name="iceQty">Default ice quantity (1-120).</param>
        /// <param name="waterQty">Default water quantity (1-10).</param>
        /// <param name="iceWaterQty">Default ice-water quantity (1-10).</param>
        /// <returns>The command byte array.</returns>
        public byte[] SetParametersCommand(IceMakerLanguage? language, byte? iceQty, byte? waterQty, byte? iceWaterQty)
        {
            // Note: Add validation for quantity ranges if needed
            byte[] reserved = new byte[6]; // D1-D6, send as 0x00
            var updateLanguage = language ?? IceMakerLanguage.English;
            var updateIceQty = iceQty ?? 1;
            var updateWaterQty = waterQty ?? 1;
            var updateIceWaterQty = iceWaterQty ?? 1;


            return commandBuilder
                .AddCommandCode(IceMakerCMCode.Cmd_ParameterQuerySet)
                .AddInstructionCode(IceMakerCMCode.Inst_SetOrAction)
                .AddData(reserved) // D1-D6
                .AddData((byte)updateLanguage) // D7
                .AddData(updateIceQty) // D8
                .AddData(updateWaterQty) // D9
                .AddData(updateIceWaterQty) // D10
                .Build(); // Length=15
        }

        /// <summary>
        /// Parses the response byte array for a Parameter Set action.
        /// Structure: [0x02] [0x06] [0xAA] [Data1(Result)] [Checksum] [0xFF] (Length=6)
        /// </summary>
        /// <param name="responseData">The raw byte array received from the slave.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool HandleSetResponse(byte[] responseData)
        {
            const int expectedLength = 6;
            if (responseData == null || responseData.Length != expectedLength) return false;
            if (responseData[0] != IceMakerCMCode.Cmd_ParameterQuerySet) return false;
            if (responseData[1] != expectedLength) return false;
            if (responseData[2] != IceMakerCMCode.Inst_SetOrAction) return false;
            if (responseData[expectedLength - 1] != IceMakerCMCode.EndCodeValue) return false;

            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];

            SetResult = Enum.IsDefined(typeof(OperationResult), responseData[3])
                ? (OperationResult)responseData[3]
                : OperationResult.Failed; // Treat undefined as Failed

            CheckCode = responseData[expectedLength - 2];
            EndCode = responseData[expectedLength - 1];

            return true;
        }

        /// <summary>
        /// Language options for the Ice Maker.
        /// </summary>
        public enum IceMakerLanguage : byte
        {
            Chinese = 0,
            English = 2,
            Japanese = 3,
            Unknown = 0xFF // Placeholder
        }
    }

    // ==============================================================
    // Command 0x03: Power Off (Z03 Model Only)
    // ==============================================================

    /// <summary>
    /// Handles Command 0x03: Power Off (Z03 Model Only).
    /// Note: Can only be executed when the slave is not in a Fault state.
    /// </summary>
    public class IceMakerPowerOffCommand
    {
        private readonly CommandBuilder commandBuilder;

        // Response Property
        public OperationResult Result { get; private set; } = OperationResult.Failed;

        // Common Packet Info (populated by HandleResponse)
        public byte CommandCode { get; private set; }
        public byte LengthCode { get; private set; }
        public byte InstructionCode { get; private set; }
        public byte CheckCode { get; private set; }
        public byte EndCode { get; private set; }

        public IceMakerPowerOffCommand()
        {
            commandBuilder = new CommandBuilder();
        }

        /// <summary>
        /// Gets the byte array for a Power Off request.
        /// Structure: [0x03] [0x05] [0xAA] [Checksum] [0xFF]
        /// </summary>
        /// <returns>The command byte array.</returns>
        public byte[] GetPowerOffCommand()
        {
            return commandBuilder
                .AddCommandCode(IceMakerCMCode.Cmd_PowerOff)
                .AddInstructionCode(IceMakerCMCode.Inst_SetOrAction)
                .Build(); // Length=5
        }

        /// <summary>
        /// Parses the response byte array for a Power Off action.
        /// Structure: [0x03] [0x06] [0xAA] [Data1(Result)] [Checksum] [0xFF] (Length=6)
        /// </summary>
        /// <param name="responseData">The raw byte array received from the slave.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool HandleResponse(byte[] responseData)
        {
            const int expectedLength = 6;
            if (responseData == null || responseData.Length != expectedLength) return false;
            if (responseData[0] != IceMakerCMCode.Cmd_PowerOff) return false;
            if (responseData[1] != expectedLength) return false;
            if (responseData[2] != IceMakerCMCode.Inst_SetOrAction) return false;
            if (responseData[expectedLength - 1] != IceMakerCMCode.EndCodeValue) return false;

            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];

            Result = Enum.IsDefined(typeof(OperationResult), responseData[3])
                ? (OperationResult)responseData[3]
                : OperationResult.Failed;

            CheckCode = responseData[expectedLength - 2];
            EndCode = responseData[expectedLength - 1];

            return true;
        }
    }

    // ==============================================================
    // Command 0x04: Dispense Beverage
    // ==============================================================

    /// <summary>
    /// Handles Command 0x04: Dispense Beverage.
    /// </summary>
    public class IceMakerDispenseCommand
    {
        private readonly CommandBuilder commandBuilder;

        // Response Property
        public OperationResult Result { get; private set; } = OperationResult.Failed;

        // Common Packet Info (populated by HandleResponse)
        public byte CommandCode { get; private set; }
        public byte LengthCode { get; private set; }
        public byte InstructionCode { get; private set; }
        public byte CheckCode { get; private set; }
        public byte EndCode { get; private set; }

        public IceMakerDispenseCommand()
        {
            commandBuilder = new CommandBuilder();
        }

        /// <summary>
        /// Gets the byte array for a Dispense Beverage request.
        /// Structure: [0x04] [0x07] [0xAA] [Beverage Num] [Quantity] [Checksum] [0xFF] (Length=7)
        /// </summary>
        /// <param name="beverageType">The type of beverage to dispense.</param>
        /// <param name="quantity">
        /// The amount to dispense (1-120 for Ice, 1-10 for Water/IceWater).
        /// Use 0 to dispense the default quantity stored in the machine.
        /// Note: Units are likely seconds or machine-specific units. Check "100ms version" note in doc if applicable.
        /// </param>
        /// <returns>The command byte array.</returns>
        public byte[] GetDispenseCommand(byte type, byte quantity)
        {
            // Add validation if needed:
            // if (beverageType == IceMakerBeverageType.Ice && quantity > 120) ...
            // if ((beverageType == IceMakerBeverageType.Water || beverageType == IceMakerBeverageType.IceWater) && quantity > 10 && quantity != 0) ...
            if (!Enum.IsDefined(typeof(IceMakerBeverageType), type))
            {
                throw new ArgumentException($"Invalid beverage type: {type}");
            }


            return commandBuilder
                .AddCommandCode(IceMakerCMCode.Cmd_DispenseBeverage)
                .AddInstructionCode(IceMakerCMCode.Inst_SetOrAction)
                .AddData(type)
                .AddData(quantity)
                .Build(); // Length=7
        }

        /// <summary>
        /// Parses the response byte array for a Dispense Beverage action.
        /// Structure: [0x04] [0x06] [0xAA] [Data1(Result)] [Checksum] [0xFF] (Length=6)
        /// </summary>
        /// <param name="responseData">The raw byte array received from the slave.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool HandleResponse(byte[] responseData)
        {
            const int expectedLength = 6;
            if (responseData == null || responseData.Length != expectedLength) return false;
            if (responseData[0] != IceMakerCMCode.Cmd_DispenseBeverage) return false;
            if (responseData[1] != expectedLength) return false;
            if (responseData[2] != IceMakerCMCode.Inst_SetOrAction) return false;
            if (responseData[expectedLength - 1] != IceMakerCMCode.EndCodeValue) return false;

            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];

            Result = Enum.IsDefined(typeof(OperationResult), responseData[3])
                ? (OperationResult)responseData[3]
                : OperationResult.Failed;

            CheckCode = responseData[expectedLength - 2];
            EndCode = responseData[expectedLength - 1];

            return true;
        }

        /// <summary>
        /// Defines the types of beverages that can be dispensed.
        /// </summary>
        public enum IceMakerBeverageType : byte
        {
            Ice = 1,
            Water = 2,
            IceWater = 3
        }
    }

}
using SerialDeviceConnector;

namespace CoffeeMachineController;

/// <summary>
/// "reserve" -> NOT DEFINE YET
/// </summary>
public static class CMCode
{
    #region Command Codes (Byte 1)

    /// <summary> Command: Query slave status (Host->Slave & Slave->Host) </summary>
    public const byte Cmd_StatusQuery = 0x01;

    /// <summary> Command: Query or Set slave parameters (Host->Slave & Slave->Host) </summary>
    public const byte Cmd_ParameterQuerySet = 0x02;

    /// <summary> Command: Request slave shutdown (Host->Slave) </summary>
    public const byte Cmd_Shutdown = 0x03;

    /// <summary> Command: Make Drink or Perform Cleaning/Maintenance Action (Host->Slave) </summary>
    public const byte Cmd_DrinkOrClean = 0x04;

    /// <summary> Command: Query version information (Host->Slave & Slave->Host) </summary>
    public const byte Cmd_VersionQuery = 0x05;

    /// <summary> Command: Send Event (Confirm/Cancel) (Host->Slave & Slave->Host) </summary>
    public const byte Cmd_EventCommand = 0x06;

    #endregion

    #region Instruction Codes (Byte 3)

    /// <summary> Instruction: Indicates a query operation. </summary>
    public const byte Inst_Query = 0x55;

    /// <summary> Instruction: Indicates a set, action, or command execution. </summary>
    public const byte Inst_SetOrAction = 0xAA;

    #endregion
}

#region 0x04 Drink Or Clean

public class DrinkOrCleanCommand
{
    public byte CommandCode { get; set; }
    public byte LengthCode { get; set; }
    public byte InstructionCode { get; set; }
    public byte CheckCode { get; set; }
    public byte EndCode { get; set; }
    private readonly CommandBuilder commandBuilder;

    public DrinkOrCleanCommand()
    {
        commandBuilder = new CommandBuilder();
    }

    //Request Parameters
    //--- Data 1 ---
    public byte Action { get; set; }

    //--- Drink No---
    public byte DrinkNumber { get; set; }


    //Response
    //--- Drink No---
    /// <summary>
    /// The drink number (1-100) that was requested (echoed back from host command).
    /// 0 if it wasn't a drink, in such cases, expect it to be a Clean Code
    /// </summary>
    //public byte DrinkNumber { get; set; }
    //--- Data 1 ---
    /// <summary>
    /// Indicates success or failure of the operation (Data1 in the protocol response).
    /// </summary>
    public OperationResult Result { get; set; }

    //--- Data 2 ---
    /// <summary>
    /// Specifies the reason for failure (or no error) if the operation was not successful (Data2 in protocol response).
    /// If Result is Success, this should be NoError.
    /// </summary>
    public DrinkFailureReason FailureReason { get; set; }

    public byte[] GetMakeDrinkCommand(byte action, byte drinkNumber)
    {
        //(host → slave)
        //0x04	Length code	0xAA	Data1	Drink No.	Check code	End code
        return commandBuilder
            .AddCommandCode(CMCode.Cmd_DrinkOrClean)
            .AddInstructionCode(CMCode.Inst_SetOrAction)
            .AddData(action, drinkNumber)
            .Build();
    }

    public byte[] GetCleanCommand(byte action)
    {
        //(host → slave)
        //0x04	Length code	0xAA	Data1	Drink No.	Check code	End code
        return commandBuilder
            .AddCommandCode(CMCode.Cmd_DrinkOrClean)
            .AddInstructionCode(CMCode.Inst_SetOrAction)
            .AddData(action, 0x00)
            .Build();
    }

    //(host → slave)
    //0x04 | Length code | (InstructionCode)0xAA | Data1 | Drink No. | Check code | End code
    public void HandleResponseCommand(byte[] responseData)
    {
        //(host <- slave)
        //0x04	Length code	0xAA	Drink No.	Data1	Data2	Check code	End code
        CommandCode = responseData[0];
        LengthCode = responseData[1];
        InstructionCode = responseData[2];
        DrinkNumber = responseData[3];
        Result = Enum.TryParse<OperationResult>(responseData[4].ToString(), out var drinknNumber)
            ? drinknNumber
            : default;
        FailureReason = Enum.TryParse<DrinkFailureReason>(responseData[5].ToString(), out var failureReason)
            ? failureReason
            : default;
        CheckCode = responseData[4];
        EndCode = responseData[5];
    }

    public enum CommandAction : byte
    {
        /// <summary>
        /// Value for Data1 indicating a commercial drink should be dispensed.
        /// If Data1 is this value, a valid DrinkNo (1-100) MUST be specified.
        /// </summary>
        DispenseDrink = 0x00, // If Data1 is 0, Date2 will always be 1-100, else 0 (or unused)

        /// <summary>
        /// Quick Rinse of Brewing Core (Universal)
        /// </summary>
        QuickRinseBrewCore = 0x01,

        /// <summary>
        /// Automatic flushing of milk system (301 only), clean milk store in system and outside pipe
        /// </summary>
        AutoFlushMilkSystem = 0x02,

        /// <summary>
        /// Preheating and rinsing for non-menu beverage preparation
        /// </summary>
        PreheatingAndRinsing = 0x03,

        /// <summary>
        /// Quick flushing of batching pipeline
        /// </summary>
        BatchingPipelineFlushing = 0x04,

        /// <summary>
        /// Automatic flushing of milk outlet pipe. Device where milk come out
        /// </summary>
        OutletPipelineFlushing = 0x05,

        /// <summary>
        /// Milk system quick rinse (auto confirmation), clean the pipe which suck milk
        /// </summary>
        QuickMilkRinse = 0x06
    }

    /// <summary>
    /// Specifies the reasons why the Drink or Clean operations can fail (Date2 Values).
    /// </summary>
    public enum DrinkFailureReason : byte
    {
        /// <summary>No Error: The drink was successfully prepared or cleaning action completed.</summary>
        NoError = 0x00,

        /// <summary>In abnormal/unable to produce state, mainly due to failed connection to lower computer,
        /// working state, or abnormal/faulty state (check the status).
        /// </summary>
        AbnormalState = 0x01, // Could be multiple sub-reasons

        /// <summary>Drink ID does not exist (invalid drink number requested).</summary>
        InvalidDrinkId = 0x02,

        /// <summary>The current drink needs a "milk quick rinse" before it can be made.</summary>
        NeedsMilkQuickRinse = 0x03,

        /// <summary>You need to "descale" before making it.</summary>
        NeedsDescaling = 0x04,

        /// <summary>The current drink needs to be cleaned with the "coffee system cleaning tablet" before it can be made.</summary>
        NeedsCoffeeSystemCleaningTablet = 0x05,

        /// <summary>The current drink needs to be "clean A grinder" before it can be made.</summary>
        NeedsCleanGrinderA = 0x06,

        /// <summary>The current drink needs to be "clean B grinder" before it can be made.</summary>
        NeedsCleanGrinderB = 0x07,

        /// <summary>The current drink needs to be "clean C grinder" before it can be made.</summary>
        NeedsCleanGrinderC = 0x08,

        /// <summary>The current drink needs to be cleaned with the "milk system cleaning tablet" before it can be made.</summary>
        NeedsMilkSystemCleaningTablet = 0x09,

        /// <summary>The current beverage needs to be "deep cleaning of powder systems" before it can be made.</summary>
        NeedsDeepCleanPowderSystems = 0x10,

        /// <summary>The current drink needs to be "Syrup System Cleaned" before it can be made.</summary>
        NeedsSyrupSystemCleaned = 0x11,

        /// <summary>The coffee boiler temperature does not meet the standard and coffee drinks cannot be made.</summary>
        CoffeeBoilerTempNotStandard = 0x12,

        /// <summary>The steam boiler temperature does not meet the standard and cannot make drinks with milk.</summary>
        SteamBoilerTempNotStandard = 0x13,

        /// <summary>The syrup machine is not online and cannot make syrup.</summary>
        SyrupMachineNotOnline = 0x14,

        /// <summary>Milk frother 2 is not online, and drinks with milk circuit 2 cannot be made.</summary>
        MilkFrother2NotOnline = 0x15,

        /// <summary>Milk frother 3 is not online, and drinks with milk path 3 cannot be made.</summary>
        MilkFrother3NotOnline = 0x16,

        /// <summary>A grinder is out of beans.</summary>
        GrinderAOutOfBeans = 0x17,

        /// <summary>B grinder is out of beans.</summary>
        GrinderBOutOfBeans = 0x18,

        /// <summary>C grinder is out of beans.</summary>
        GrinderCOutOfBeans = 0x19,

        /// <summary>A Bean box is out of beans.</summary>
        BeanBoxAOutOfBeans = 0x20,

        /// <summary>B Bean box is out of beans.</summary>
        BeanBoxBOutOfBeans = 0x21,

        /// <summary>C Bean box is out of beans.</summary>
        BeanBoxCOutOfBeans = 0x22,

        /// <summary>There may be foreign matter in the A grinder. Please disconnect the power supply and remove the cutter disc.</summary>
        ForeignMatterInGrinderA = 0x23,

        /// <summary>There may be foreign matter in the B grinder. Please disconnect the power supply and remove the cutter disc.</summary>
        ForeignMatterInGrinderB = 0x24,

        /// <summary>There may be foreign matter in the C grinder. Please disconnect the power supply and remove the cutter disc.</summary>
        ForeignMatterInGrinderC = 0x25,

        /// <summary>The powder box A is out of material and drinks with powder A cannot be made.</summary>
        PowderBoxAOutOfMaterial = 0x26,

        /// <summary>The powder box B is out of material and drinks with powder B cannot be made.</summary>
        PowderBoxBOutOfMaterial = 0x27,

        /// <summary>Milk line 1 is short of milk.</summary>
        MilkLine1ShortOfMilk = 0x28,

        /// <summary>Milk line 2 is short of milk.</summary>
        MilkLine2ShortOfMilk = 0x29,

        /// <summary>Milk line 3 is short of milk.</summary>
        MilkLine3ShortOfMilk = 0x30
    }
}

#endregion

#region 0x01 Slave status query

public class SlaveStatusCommand
{
    private readonly CommandBuilder commandBuilder;

    //response
    public byte CommandCode { get; set; }
    public byte LengthCode { get; set; }
    public byte InstructionCode { get; set; }
    public Data1Bit Data1 { get; set; } = default!;
    public Data2Bit Data2 { get; set; } = default!;
    public Data3Bit Data3 { get; set; } = default!;

    public Data4Bit Data4 { get; set; } = default!;

    //Data 5
    public byte ProductionProgress { get; set; } //The current production progress of the drink (0~100)
    public Data6Bit Data6 { get; set; } = default!;
    public Data7Bit Data7 { get; set; } = default!;
    public Data8Bit Data8 { get; set; } = default!;
    public Data9Bit Data9 { get; set; } = default!;
    public Data10Bit Data10 { get; set; } = default!;
    public Data11Bit Data11 { get; set; } = default!;
    public Data12Bit Data12 { get; set; } = default!;
    public Data13Bit Data13 { get; set; } = default!;
    public Data14Bit Data14 { get; set; } = default!;
    public Data15Bit Data15 { get; set; } = default!;
    public Data16Bit Data16 { get; set; } = default!;
    public Data17Bit Data17 { get; set; } = default!;
    public Data18Bit Data18 { get; set; } = default!;
    public Data19Bit Data19 { get; set; } = default!;
    public Data20Bit Data20 { get; set; } = default!;
    public Data21Bit Data21 { get; set; } = default!;


    public byte CheckCode { get; set; }
    public byte EndCode { get; set; }

    public SlaveStatusCommand()
    {
        commandBuilder = new CommandBuilder();
    }

    public byte[] GetQueryStatusCommand()
    {
        //(host → slave）
        //Send Command: (Command Code)0x01 | Lengthcode | InstructionCode(0x55)| Check code | End code
        return commandBuilder
            .AddCommandCode(CMCode.Cmd_StatusQuery)
            .AddInstructionCode(CMCode.Inst_Query)
            .Build();
    }

    public void HandleSResponse(byte[] responseData)
    {
        try
        {
            //(host←slave)
            //Receive Command: (Command Code)0x01 |	Lengthcode | 0x55 |	Data 1	Data 2	Data 3	Data 4....Data 19 |	Check code | End code
            CommandCode = responseData[0];
            LengthCode = responseData[1];
            InstructionCode = responseData[2];
            Data1 = new Data1Bit(responseData[3]);
            Data2 = new Data2Bit(responseData[4]);
            Data3 = new Data3Bit(responseData[5]);
            Data4 = new Data4Bit(responseData[6]);
            ProductionProgress = responseData[7];
            Data6 = new Data6Bit(responseData[8]);
            Data7 = new Data7Bit(responseData[9]);
            Data8 = new Data8Bit(responseData[10]);
            Data9 = new Data9Bit(responseData[11]);
            Data10 = new Data10Bit(responseData[12]);
            Data11 = new Data11Bit(responseData[13]);
            Data12 = new Data12Bit(responseData[14]);
            Data13 = new Data13Bit(responseData[15]);
            Data14 = new Data14Bit(responseData[16]);
            Data15 = new Data15Bit(responseData[17]);
            Data16 = new Data16Bit(responseData[18]);
            Data17 = new Data17Bit(responseData[19]);
            Data18 = new Data18Bit(responseData[20]);
            Data19 = new Data19Bit(responseData[21]);
            Data20 = new Data20Bit(responseData[22]);
            Data21 = new Data21Bit(responseData[23]);
            CheckCode = responseData[24];
            EndCode = responseData[25];
        }
        catch (InvalidOperationException ioe)
        {
            throw new InvalidOperationException("Invalid response received from the coffee machine.");
        }
        catch (Exception e)
        {
            throw;
        }

    }
    /// <summary>
    /// Represents the high-level system status (Data 1, Bits 1-2).
    /// </summary>
    public enum SystemStatus
    {
        Initialization = 0b00, //Initialization status (see data 5 for sub-status)
        Idle = 0b01, //Idle status
        Running = 0b10, //Running status (see data 5, data 6, data 7 for sub-status)
        Shutdown = 0b11, //Shutdown status
        Unknown = -1 // For error cases
    }
    #region Data Bit Classes

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

    public class Data1Bit : DataBit
    {
        public Data1Bit(byte data)
        {
            LoadFromByte(data);
            CurrentSystemStatus = (SystemStatus)(data >> 1 & 0b11);
        }

        //Bit 0
        public bool HasFault => Bits[0]; //1: Fault (fault code see data 2, 3, 4)

        //0: Normal
        // Bit2:1
        public SystemStatus CurrentSystemStatus { get; set; }

        // Bit 3
        public bool IsCoffeeBoilerDisconnected => Bits[3]; //1: Coffee boiler disconnected

        //0: Normal
        // Bit 4
        public bool IsSteamBoilerDisconnected => Bits[4]; //1: Steam boiler disconnected

        //0: Normal
        // Bit 5
        public bool IsCoffeeBoilerNtcFault => Bits[5]; //1: Coffee boiler NTC fault

        //0: Normal
        // Bit 6
        public bool IsSteamBoilerNtcFault => Bits[6]; //1: Steam boiler NTC fault

        //0: Normal
        // Bit 7
        public bool IsCoffeeBoilerTempTooLow => Bits[7]; //1: Coffee boiler temperature is too low
        //0: Normal
    }

    public class Data2Bit : DataBit
    {
        public Data2Bit(byte data)
        {
            LoadFromByte(data);
        }

        // Bit 0
        public bool IsSteamBoilerTempTooLow => Bits[0]; // 1: The steam boiler temperature is too low

        // 0: normal
        // Bit 1
        public bool IsCoffeeBoilerTempTooHigh => Bits[1]; // 1: The coffee boiler temperature is too high

        // 0: normal
        // Bit 2
        public bool IsSteamBoilerTempTooHigh => Bits[2]; // 1: The steam boiler temperature is too high
        // 0: normal
        // Bit 3 reserved -> NOT DEFINE YET

        // Bit 4
        public bool IsCoffeePipeBlocked => Bits[4]; // 1: The coffee pipe is blocked

        // 0: normal
        // Bit 5
        public bool IsNormalTempWaterPipeBlocked => Bits[5]; // 1: The normal temperature water pipeline is blocked

        // 0: normal
        // Bit 6
        public bool IsGrinder1SystemAbnormal => Bits[6]; // 1: No. 1 grinding system is abnormal

        // 0: normal
        // Bit 7
        public bool IsGrinder2SystemAbnormal => Bits[7]; // 1: No. 2 grinding system is abnormal
        // 0: normal
    }

    public class Data3Bit : DataBit
    {
        public Data3Bit(byte data)
        {
            LoadFromByte(data);
        }
        // --- Data 3 (Fault Codes) ---

        // Bit 0
        public bool IsBean1Empty => Bits[0]; // 1: No. 1 coffee beans are used up

        // 0: normal
        // Bit 1
        public bool IsBean2Empty => Bits[1]; // 1: No. 2 coffee beans are used up

        // 0: normal
        // Bit 2
        public bool IsDeliveryPort1SwitchAbnormal => Bits[2]; // 1:1 The switch of the delivery port is abnormal

        // 0: normal
        // Bit 3
        public bool IsDeliveryPort2SwitchAbnormal => Bits[3]; // 1:2 The switch of the delivery port is abnormal

        // 0: normal
        // Bit 4
        public bool IsIngredient1Empty => Bits[4]; // 1: Ingredient 1 is used up

        // 0: normal
        // Bit 5
        public bool IsIngredient2Empty => Bits[5]; // 1: Ingredient 2 is used up

        // 0: normal
        // Bit 6
        public bool IsWaterInletPressureAbnormal => Bits[6]; // 1: Abnormal water inlet pressure

        // 0: normal
        // Bit 7
        public bool IsBrewDoorOpen => Bits[7]; // 1: Please close the brew door
        // 0: normal
    }

    // --- Data 4 (Fault Codes) ---
    public class Data4Bit : DataBit
    {
        public Data4Bit(byte data)
        {
            LoadFromByte(data);
        }

        // Bit 0
        public bool IsBrewerNotInstalled => Bits[0]; // 1: The brewer is not installed

        // 0: normal
        // Bit 1
        public bool IsMilkChannel3Empty => Bits[1]; // 1: Milk channel 3 is lacking milk

        // 0: normal
        // Bit 2
        public bool IsMilkChannel1Empty => Bits[2]; // 1: Milk channel 1 is short of milk

        // 0: normal
        // Bit 3
        public bool IsWaterStoragePanNeeded => Bits[3]; // 1: A water storage pan needs to be installed

        // 0: normal
        // Bit 4
        public bool IsDripTrayFull => Bits[4]; // 1: The water tank is full // Using your property name 'IsDripTrayFull'

        // 0: normal
        // Bit 5
        public bool IsWasteBinFull => Bits[5]; // 1: Waste slag is full // Using your property name 'IsWasteBinFull'

        // 0: normal
        // Bit 6
        public bool IsBrewerDeviceFailure => Bits[6]; // 1: Brewing device failure

        // 0: normal
        // Bit 7
        public bool IsBrewingPressureTooHigh => Bits[7]; // 1: Brewing pressure is too high
        // 0: normal
    }


    // --- Data 5 ---
    //public byte ProductionProgress { get; set; } //The current production progress of the drink (0~100)

    // --- Data 6 (Fault Codes) ---
    public class Data6Bit : DataBit
    {
        public Data6Bit(byte data)
        {
            LoadFromByte(data);
        }

        // Bit 0
        public bool IsBean3Empty => Bits[0]; // 1: No. 3 coffee beans are used up

        // 0: normal
        // Bit 1
        public bool IsStirrerNotInstalled => Bits[1]; // 1: The stirrer is not installed

        // 0: normal
        // Bit 2
        public bool IsInstantBoilerDisconnected => Bits[2]; // 1: The instant coffee boiler is disconnected

        // 0: normal
        // Bit 3
        public bool IsInstantBoilerNtcFault => Bits[3]; // 1: Instant coffee boiler NTC failure

        // 0: normal
        // Bit 4
        public bool IsInstantBoilerTempTooLow => Bits[4]; // 1: The temperature of the instant coffee boiler is too low

        // 0: normal
        // Bit 5
        public bool IsInstantBoilerTempTooHigh =>
            Bits[5]; // 1: The temperature of the instant coffee boiler is too high

        // 0: normal
        // Bit 6
        public bool IsDeliveryPortFailure => Bits[6]; // 1: Failure of the delivery port

        // 0: reserve // Note: Document marks '0' as reserve
        // Bit 7
        public bool IsMilkChannel2Empty => Bits[7]; // 1: Milk channel 2 is lacking milk
        // 0: reserve // Note: Document marks '0' as reserve
    }

    public class Data7Bit : DataBit
    {
        public Data7Bit(byte data)
        {
            LoadFromByte(data);
        }

        // Bit 0
        public bool IsDrinkId1Unavailable => Bits[0]; // 1: Drink id1 cannot be made

        // 0: normal
        // Bit 1
        public bool IsDrinkId2Unavailable => Bits[1]; // 1: Drink id2 cannot be made

        // 0: normal
        // Bit 2
        public bool IsDrinkId3Unavailable => Bits[2]; // 1: Drink id3 cannot be made

        // 0: normal
        // Bit 3
        public bool IsDrinkId4Unavailable => Bits[3]; // 1: Drink id4 cannot be made

        // 0: normal
        // Bit 4
        public bool IsDrinkId5Unavailable => Bits[4]; // 1: Drink id5 cannot be made

        // 0: normal
        // Bit 5
        public bool IsDrinkId6Unavailable => Bits[5]; // 1: Drink id6 cannot be made

        // 0: normal
        // Bit 6
        public bool IsDrinkId7Unavailable => Bits[6]; // 1: Drink id7 cannot be made

        // 0: normal
        // Bit 7
        public bool IsDrinkId8Unavailable => Bits[7]; // 1: Drink id8 cannot be made
        // 0: normal
    }

    public class Data8Bit : DataBit
    {
        public Data8Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId9Unavailable => Bits[0];
        public bool IsDrinkId10Unavailable => Bits[1];
        public bool IsDrinkId11Unavailable => Bits[2];
        public bool IsDrinkId12Unavailable => Bits[3];
        public bool IsDrinkId13Unavailable => Bits[4];
        public bool IsDrinkId14Unavailable => Bits[5];
        public bool IsDrinkId15Unavailable => Bits[6];
        public bool IsDrinkId16Unavailable => Bits[7];
    }

    public class Data9Bit : DataBit
    {
        public Data9Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId17Unavailable => Bits[0];
        public bool IsDrinkId18Unavailable => Bits[1];
        public bool IsDrinkId19Unavailable => Bits[2];
        public bool IsDrinkId20Unavailable => Bits[3];
        public bool IsDrinkId21Unavailable => Bits[4];
        public bool IsDrinkId22Unavailable => Bits[5];
        public bool IsDrinkId23Unavailable => Bits[6];
        public bool IsDrinkId24Unavailable => Bits[7];
    }

    public class Data10Bit : DataBit
    {
        public Data10Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId25Unavailable => Bits[0];
        public bool IsDrinkId26Unavailable => Bits[1];
        public bool IsDrinkId27Unavailable => Bits[2];
        public bool IsDrinkId28Unavailable => Bits[3];
        public bool IsDrinkId29Unavailable => Bits[4];
        public bool IsDrinkId30Unavailable => Bits[5];
        public bool IsDrinkId31Unavailable => Bits[6];
        public bool IsDrinkId32Unavailable => Bits[7];
    }

    public class Data11Bit : DataBit
    {
        public Data11Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId33Unavailable => Bits[0];
        public bool IsDrinkId34Unavailable => Bits[1];
        public bool IsDrinkId35Unavailable => Bits[2];
        public bool IsDrinkId36Unavailable => Bits[3];
        public bool IsDrinkId37Unavailable => Bits[4];
        public bool IsDrinkId38Unavailable => Bits[5];
        public bool IsDrinkId39Unavailable => Bits[6];
        public bool IsDrinkId40Unavailable => Bits[7];
    }

    public class Data12Bit : DataBit
    {
        public Data12Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId41Unavailable => Bits[0];
        public bool IsDrinkId42Unavailable => Bits[1];
        public bool IsDrinkId43Unavailable => Bits[2];
        public bool IsDrinkId44Unavailable => Bits[3];
        public bool IsDrinkId45Unavailable => Bits[4];
        public bool IsDrinkId46Unavailable => Bits[5];
        public bool IsDrinkId47Unavailable => Bits[6];
        public bool IsDrinkId48Unavailable => Bits[7];
    }

    public class Data13Bit : DataBit
    {
        public Data13Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId49Unavailable => Bits[0];
        public bool IsDrinkId50Unavailable => Bits[1];
        public bool IsDrinkId51Unavailable => Bits[2];
        public bool IsDrinkId52Unavailable => Bits[3];
        public bool IsDrinkId53Unavailable => Bits[4];
        public bool IsDrinkId54Unavailable => Bits[5];
        public bool IsDrinkId55Unavailable => Bits[6];
        public bool IsDrinkId56Unavailable => Bits[7];
    }

    public class Data14Bit : DataBit
    {
        public Data14Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId57Unavailable => Bits[0];
        public bool IsDrinkId58Unavailable => Bits[1];
        public bool IsDrinkId59Unavailable => Bits[2];
        public bool IsDrinkId60Unavailable => Bits[3];
        public bool IsDrinkId61Unavailable => Bits[4];
        public bool IsDrinkId62Unavailable => Bits[5];
        public bool IsDrinkId63Unavailable => Bits[6];
        public bool IsDrinkId64Unavailable => Bits[7];
    }

    public class Data15Bit : DataBit
    {
        public Data15Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId65Unavailable => Bits[0];
        public bool IsDrinkId66Unavailable => Bits[1];
        public bool IsDrinkId67Unavailable => Bits[2];
        public bool IsDrinkId68Unavailable => Bits[3];
        public bool IsDrinkId69Unavailable => Bits[4];
        public bool IsDrinkId70Unavailable => Bits[5];
        public bool IsDrinkId71Unavailable => Bits[6];
        public bool IsDrinkId72Unavailable => Bits[7];
    }

    public class Data16Bit : DataBit
    {
        public Data16Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId73Unavailable => Bits[0];
        public bool IsDrinkId74Unavailable => Bits[1];
        public bool IsDrinkId75Unavailable => Bits[2];
        public bool IsDrinkId76Unavailable => Bits[3];
        public bool IsDrinkId77Unavailable => Bits[4];
        public bool IsDrinkId78Unavailable => Bits[5];
        public bool IsDrinkId79Unavailable => Bits[6];
        public bool IsDrinkId80Unavailable => Bits[7];
    }

    public class Data17Bit : DataBit
    {
        public Data17Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId81Unavailable => Bits[0];
        public bool IsDrinkId82Unavailable => Bits[1];
        public bool IsDrinkId83Unavailable => Bits[2];
        public bool IsDrinkId84Unavailable => Bits[3];
        public bool IsDrinkId85Unavailable => Bits[4];
        public bool IsDrinkId86Unavailable => Bits[5];
        public bool IsDrinkId87Unavailable => Bits[6];
        public bool IsDrinkId88Unavailable => Bits[7];
    }

    public class Data18Bit : DataBit
    {
        public Data18Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId89Unavailable => Bits[0];
        public bool IsDrinkId90Unavailable => Bits[1];
        public bool IsDrinkId91Unavailable => Bits[2];
        public bool IsDrinkId92Unavailable => Bits[3];
        public bool IsDrinkId93Unavailable => Bits[4];
        public bool IsDrinkId94Unavailable => Bits[5];
        public bool IsDrinkId95Unavailable => Bits[6];
        public bool IsDrinkId96Unavailable => Bits[7];
    }

    public class Data19Bit : DataBit
    {
        public Data19Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsDrinkId89Unavailable => Bits[0];
        public bool IsDrinkId90Unavailable => Bits[1];
        public bool IsDrinkId91Unavailable => Bits[2];
        public bool IsDrinkId92Unavailable => Bits[3];
        public bool IsDrinkId93Unavailable => Bits[4];
        public bool IsDrinkId94Unavailable => Bits[5];
        public bool IsDrinkId95Unavailable => Bits[6];
        public bool IsDrinkId96Unavailable => Bits[7];
    }

    // --- Data 20 (Fault Codes - Added in V0.09/V0.10) ---
    public class Data20Bit : DataBit
    {
        public Data20Bit(byte data)
        {
            LoadFromByte(data);
        }

        // Bit 0
        public bool IsBrewerMotorOverheated => Bits[0];

        // 1: The brewer motor is overheated
        // 0: normal
        // Bit 1
        public bool IsPipelineLeakage => Bits[1]; // 1: Pipeline leakage

        // 0: normal
        // Bit 2
        public bool IsBeanBox1PhotoelectricLow => Bits[2]; // 1: Bean box photoelectric detection1

        // 0: normal
        // Bit 3
        public bool IsBeanBox2PhotoelectricLow => Bits[3]; // 1: Bean box photoelectric detection2

        // 0: normal
        // Bit 4
        public bool IsBeanBox3PhotoelectricLow => Bits[4]; // 1: Bean box photoelectric detection3

        // 0: normal
        // Bit 5
        public bool IsBeanBox1NotInstalled => Bits[5]; // 1: Bean box 1 is not installed

        // 0: normal
        // Bit 6
        public bool IsBeanBox2NotInstalled => Bits[6]; // 1: Bean box 2 is not installed

        // 0: reserve 
        // Bit 7
        public bool IsBeanBox3NotInstalled => Bits[7]; // 1: Bean box 3 is not installed
        // 0: reserve 
    }

    public class Data21Bit : DataBit
    {
        public Data21Bit(byte data)
        {
            LoadFromByte(data);
        }

        public bool IsBeanBinHandleNotInPlace => Bits[0]; // 1: The bean bin handle is not in place 0：normal
        public bool IsPowderMixingBinNotInstalled => Bits[1]; // 1: The powder mixing bin is not installed 0：normal
        public bool IsPhotoelectricModuleFailure => Bits[2]; // 1: Photoelectric module failure 0：normal

        // Bits 3-7 are reserved: No properties are created for reserved bits
    }

    #endregion
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




    public byte[] GetShutDownCommand()
    {
        //(host → slave)
        //(Command Code)0x03 | Length code | (InstructionCode)0xAA | Check code | End code
        return commandBuilder
            .AddCommandCode(CMCode.Cmd_Shutdown)
            .AddInstructionCode(CMCode.Inst_SetOrAction)
            .Build();
    }

    public void HandleResponseCommand(byte[] responseData)
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

#region 0x06 Event Handler
public class EventHandlerCommand
{
    private readonly CommandBuilder commandBuilder;
    public EventHandlerCommand()
    {
        commandBuilder = new CommandBuilder();
    }

    public byte CommandCode { get; set; }
    public byte LengthCode { get; set; }
    public byte InstructionCode { get; set; }
    public byte CheckCode { get; set; }
    public byte EndCode { get; set; }
    //Request 
    //* Event Command
    public EventCommandEnum EventCommand { get; set; }
    //* Data1: event command for which event
    public TargetEvent Target { get; set; }

    //Response
    //* Event Command
    public EventCommandEnum EventCommandRes { get; set; }
    //* Data 1
    /*
        0x00	Setup failed
        0x01	Set up for success
     */
    public OperationResult Result { get; set; }
    public byte[] GetEventHandlerCommand(byte eventCommand, byte target)
    {
        //(host → slave)
        //(Command Code)0x06 | Length code | (InstructionCode)0xAA | EventCommand | Data1 | Check code | End code
        return commandBuilder
            .AddCommandCode(CMCode.Cmd_EventCommand)
            .AddInstructionCode(CMCode.Inst_SetOrAction)
            .AddData([eventCommand, target])
            .Build();
    }

    public void HandleSResponse(byte[] responseData)
    {
        //(host←slave)
        //Receive Command: (Command Code)0x06 |	Lengthcode | 0xAA |	EventCommand | Data1 | Check code | End code
        CommandCode = responseData[0];
        LengthCode = responseData[1];
        InstructionCode = responseData[2];
        EventCommandRes = Enum.TryParse<EventCommandEnum>(responseData[3].ToString(), out var eventCmd) ? eventCmd : default;
        Result = Enum.TryParse<OperationResult>(responseData[4].ToString(), out var result) ? result : default;
        CheckCode = responseData[5];
        EndCode = responseData[6];
    }

    public enum EventCommandEnum
    {
        /// <summary>Confirm event command</summary>
        Failed = 0x01,

        /// <summary>Cancle event command.</summary>
        Success = 0x02
    }

    public enum TargetEvent
    {
        /// <summary>General event command (try to fix/continue).</summary>
        General = 0x00,

        /// <summary>Bean bin No.1 is out of beans.</summary>
        BeanBin1OutOfBeans = 0x01,

        /// <summary>Bean bin No.2 is out of beans.</summary>
        BeanBin2OutOfBeans = 0x02,

        /// <summary>Bean bin No.3 is out of beans.</summary>
        BeanBin3OutOfBeans = 0x03,

        /// <summary>Fancy milk system No.1 is out of milk.</summary>
        FancyMilk1OutOfMilk = 0x04,

        /// <summary>Fancy milk system No.2 is out of milk.</summary>
        FancyMilk2OutOfMilk = 0x05,

        /// <summary>Fancy milk system No.3 is out of milk.</summary>
        FancyMilk3OutOfMilk = 0x06,

        /// <summary>Powder system No.1 is out of powder.</summary>
        Powder1OutOfPowder = 0x07,

        /// <summary>Powder system No.2 is out of powder.</summary>
        Powder2OutOfPowder = 0x08,

        /// <summary>Photoelectric detection of bean bin No.1 is insufficient.</summary>
        BeanBin1PhotoelectricLow = 0x09,

        /// <summary>Photoelectric detection of bean bin No.2 is insufficient.</summary>
        BeanBin2PhotoelectricLow = 0x0A,

        /// <summary>Photoelectric detection of bean bin No.3 is insufficient.</summary>
        BeanBin3PhotoelectricLow = 0x0B
    }

}
#endregion

/// <summary>
/// Indicates whether the command/operation succeeded or failed (Date1 Values)
/// </summary>
public enum OperationResult : byte
{
    /// <summary>The requested operation failed.</summary>
    Failed = 0x00,

    /// <summary>The requested operation succeeded.</summary>
    Success = 0x01
}


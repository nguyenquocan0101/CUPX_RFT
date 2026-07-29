/// <summary>
/// Constants for IceMaker serial communication protocol V0.0.3
/// </summary>

namespace IceMakerMachineTest;

public static class IceMakerConstants
{

    public static class QuantityLimits
    {
        public const byte MinIceQuantity = 0x01;
        public const byte MaxIceQuantity = 120;
        public const byte MinWaterQuantity = 0x01;
        public const byte MaxWaterQuantity = 0x10;
        public const byte MinIceWaterQuantity = 0x01;
        public const byte MaxIceWaterQuantity = 0x10;
    }
    
}
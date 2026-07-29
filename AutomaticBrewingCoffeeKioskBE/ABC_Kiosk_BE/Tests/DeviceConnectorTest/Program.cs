using System;
//using Services.Models.DeviceConnectors;
//using Services.Models.DeviceConnectors.SerialDeviceConnector;
using Services.Utils;

class Program
{
    static void Main()
    {
        //CoffeeMachine coffeeMachine = new CoffeeMachine("COM13", 115200);

        //try
        //{
        //    coffeeMachine.Connect();
        //    Console.WriteLine("Connected to Coffee Machine.");

        //    while (true)
        //    {
        //        Console.WriteLine("\nSelect an option:");
        //        Console.WriteLine("1. Query Machine Status");
        //        Console.WriteLine("2. Make a Drink");
        //        Console.WriteLine("3. Clean Machine");
        //        Console.WriteLine("4. Exit");
        //        Console.Write("Enter your choice: ");

        //        string choice = Console.ReadLine();
        //        switch (choice)
        //        {
        //            case "1":
        //                QueryStatus(coffeeMachine);


        //                break;
        //            case "2":
        //                MakeDrink(coffeeMachine);


        //                break;
        //            case "3":
        //                CleanMachine(coffeeMachine);


        //                break;
        //            case "4":
        //                Console.WriteLine("Disconnecting...");
        //                coffeeMachine.Disconnect();
        //                return;
        //            default:
        //                Console.WriteLine("Invalid choice. Try again.");
        //                break;
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error: {ex.Message}");
        //}
    }

    //static void QueryStatus(CoffeeMachine coffeeMachine)
    //{
    //    var slaveStatus = coffeeMachine.QueryStatus();
    //    if (slaveStatus != null)
    //    {
    //        Console.WriteLine("--- Slave Status Report ---");
    //        Console.WriteLine($"Command Code: 0x{slaveStatus.CommandCode:X2}");
    //        Console.WriteLine($"Length Code: 0x{slaveStatus.LengthCode:X2}");
    //        Console.WriteLine($"Instruction Code: 0x{slaveStatus.InstructionCode:X2}");

    //        Console.WriteLine("--- Data ---");
    //        Console.WriteLine($"  [Data1] Has Fault: {slaveStatus.Data1.HasFault}, Current System Status: {slaveStatus.Data1.CurrentSystemStatus}, Coffee Boiler Disconnected: {slaveStatus.Data1.IsCoffeeBoilerDisconnected}, Steam Boiler Disconnected: {slaveStatus.Data1.IsSteamBoilerDisconnected}, Coffee Boiler NTC Fault: {slaveStatus.Data1.IsCoffeeBoilerNtcFault}, Steam Boiler NTC Fault: {slaveStatus.Data1.IsSteamBoilerNtcFault}, Coffee Boiler Temp Too Low: {slaveStatus.Data1.IsCoffeeBoilerTempTooLow}");

    //        Console.WriteLine($"  [Data2] Steam Boiler Temp Too Low: {slaveStatus.Data2.IsSteamBoilerTempTooLow}, Coffee Boiler Temp Too High: {slaveStatus.Data2.IsCoffeeBoilerTempTooHigh}, Steam Boiler Temp Too High: {slaveStatus.Data2.IsSteamBoilerTempTooHigh}, Coffee Pipe Blocked: {slaveStatus.Data2.IsCoffeePipeBlocked}, Normal Temp Water Pipe Blocked: {slaveStatus.Data2.IsNormalTempWaterPipeBlocked}, Grinder1 System Abnormal: {slaveStatus.Data2.IsGrinder1SystemAbnormal}, Grinder2 System Abnormal: {slaveStatus.Data2.IsGrinder2SystemAbnormal}");

    //        Console.WriteLine($"  [Data3] Bean1 Empty: {slaveStatus.Data3.IsBean1Empty}, Bean2 Empty: {slaveStatus.Data3.IsBean2Empty}, DeliveryPort1SwitchAbnormal: {slaveStatus.Data3.IsDeliveryPort1SwitchAbnormal}, DeliveryPort2SwitchAbnormal: {slaveStatus.Data3.IsDeliveryPort2SwitchAbnormal}, Ingredient1 Empty: {slaveStatus.Data3.IsIngredient1Empty}, Ingredient2 Empty: {slaveStatus.Data3.IsIngredient2Empty}, WaterInletPressureAbnormal: {slaveStatus.Data3.IsWaterInletPressureAbnormal}, BrewDoorOpen: {slaveStatus.Data3.IsBrewDoorOpen}");

    //        Console.WriteLine($"  [Data4] Brewer Not Installed: {slaveStatus.Data4.IsBrewerNotInstalled}, MilkChannel3Empty: {slaveStatus.Data4.IsMilkChannel3Empty}, MilkChannel1Empty: {slaveStatus.Data4.IsMilkChannel1Empty}, WaterStoragePanNeeded: {slaveStatus.Data4.IsWaterStoragePanNeeded}, DripTrayFull: {slaveStatus.Data4.IsDripTrayFull}, WasteBinFull: {slaveStatus.Data4.IsWasteBinFull}, BrewerDeviceFailure: {slaveStatus.Data4.IsBrewerDeviceFailure}, BrewingPressureTooHigh: {slaveStatus.Data4.IsBrewingPressureTooHigh}");

    //        Console.WriteLine($"  Production Progress: {slaveStatus.ProductionProgress}%");

    //        Console.WriteLine($"  [Data6] Bean3 Empty: {slaveStatus.Data6.IsBean3Empty}, Stirrer Not Installed: {slaveStatus.Data6.IsStirrerNotInstalled}, InstantBoilerDisconnected: {slaveStatus.Data6.IsInstantBoilerDisconnected}, InstantBoilerNtcFault: {slaveStatus.Data6.IsInstantBoilerNtcFault}, InstantBoilerTempTooLow: {slaveStatus.Data6.IsInstantBoilerTempTooLow}, InstantBoilerTempTooHigh: {slaveStatus.Data6.IsInstantBoilerTempTooHigh}, DeliveryPortFailure: {slaveStatus.Data6.IsDeliveryPortFailure}, MilkChannel2Empty: {slaveStatus.Data6.IsMilkChannel2Empty}");

    //        Console.WriteLine($"  [Data7] DrinkId1Unavailable: {slaveStatus.Data7.IsDrinkId1Unavailable}, DrinkId2Unavailable: {slaveStatus.Data7.IsDrinkId2Unavailable}, DrinkId3Unavailable: {slaveStatus.Data7.IsDrinkId3Unavailable}, DrinkId4Unavailable: {slaveStatus.Data7.IsDrinkId4Unavailable}, DrinkId5Unavailable: {slaveStatus.Data7.IsDrinkId5Unavailable}, DrinkId6Unavailable: {slaveStatus.Data7.IsDrinkId6Unavailable}, DrinkId7Unavailable: {slaveStatus.Data7.IsDrinkId7Unavailable}, DrinkId8Unavailable: {slaveStatus.Data7.IsDrinkId8Unavailable}");

    //        Console.WriteLine($"  [Data8] DrinkId9Unavailable: {slaveStatus.Data8.IsDrinkId9Unavailable}, DrinkId10Unavailable: {slaveStatus.Data8.IsDrinkId10Unavailable}, DrinkId11Unavailable: {slaveStatus.Data8.IsDrinkId11Unavailable}, DrinkId12Unavailable: {slaveStatus.Data8.IsDrinkId12Unavailable}, DrinkId13Unavailable: {slaveStatus.Data8.IsDrinkId13Unavailable}, DrinkId14Unavailable: {slaveStatus.Data8.IsDrinkId14Unavailable}, DrinkId15Unavailable: {slaveStatus.Data8.IsDrinkId15Unavailable}, DrinkId16Unavailable: {slaveStatus.Data8.IsDrinkId16Unavailable}");

    //        Console.WriteLine($"  [Data9] DrinkId17Unavailable: {slaveStatus.Data9.IsDrinkId17Unavailable}, DrinkId18Unavailable: {slaveStatus.Data9.IsDrinkId18Unavailable}, DrinkId19Unavailable: {slaveStatus.Data9.IsDrinkId19Unavailable}, DrinkId20Unavailable: {slaveStatus.Data9.IsDrinkId20Unavailable}, DrinkId21Unavailable: {slaveStatus.Data9.IsDrinkId21Unavailable}, DrinkId22Unavailable: {slaveStatus.Data9.IsDrinkId22Unavailable}, DrinkId23Unavailable: {slaveStatus.Data9.IsDrinkId23Unavailable}, DrinkId24Unavailable: {slaveStatus.Data9.IsDrinkId24Unavailable}");

    //        Console.WriteLine($" [Data10] DrinkId25Unavailable: {slaveStatus.Data10.IsDrinkId25Unavailable}, DrinkId26Unavailable: {slaveStatus.Data10.IsDrinkId26Unavailable}, DrinkId27Unavailable: {slaveStatus.Data10.IsDrinkId27Unavailable}, DrinkId28Unavailable: {slaveStatus.Data10.IsDrinkId28Unavailable}, DrinkId29Unavailable: {slaveStatus.Data10.IsDrinkId29Unavailable}, DrinkId30Unavailable: {slaveStatus.Data10.IsDrinkId30Unavailable}, DrinkId31Unavailable: {slaveStatus.Data10.IsDrinkId31Unavailable}, DrinkId32Unavailable: {slaveStatus.Data10.IsDrinkId32Unavailable}");

    //        Console.WriteLine($" [Data11] DrinkId33Unavailable: {slaveStatus.Data11.IsDrinkId33Unavailable}, DrinkId34Unavailable: {slaveStatus.Data11.IsDrinkId34Unavailable}, DrinkId35Unavailable: {slaveStatus.Data11.IsDrinkId35Unavailable}, DrinkId36Unavailable: {slaveStatus.Data11.IsDrinkId36Unavailable}, DrinkId37Unavailable: {slaveStatus.Data11.IsDrinkId37Unavailable}, DrinkId38Unavailable: {slaveStatus.Data11.IsDrinkId38Unavailable}, DrinkId39Unavailable: {slaveStatus.Data11.IsDrinkId39Unavailable}, DrinkId40Unavailable: {slaveStatus.Data11.IsDrinkId40Unavailable}");

    //        Console.WriteLine($" [Data12] DrinkId41Unavailable: {slaveStatus.Data12.IsDrinkId41Unavailable}, DrinkId42Unavailable: {slaveStatus.Data12.IsDrinkId42Unavailable}, DrinkId43Unavailable: {slaveStatus.Data12.IsDrinkId43Unavailable}, DrinkId44Unavailable: {slaveStatus.Data12.IsDrinkId44Unavailable}, DrinkId45Unavailable: {slaveStatus.Data12.IsDrinkId45Unavailable}, DrinkId46Unavailable: {slaveStatus.Data12.IsDrinkId46Unavailable}, DrinkId47Unavailable: {slaveStatus.Data12.IsDrinkId47Unavailable}, DrinkId48Unavailable: {slaveStatus.Data12.IsDrinkId48Unavailable}");

    //        Console.WriteLine($" [Data13] DrinkId49Unavailable: {slaveStatus.Data13.IsDrinkId49Unavailable}, DrinkId50Unavailable: {slaveStatus.Data13.IsDrinkId50Unavailable}, DrinkId51Unavailable: {slaveStatus.Data13.IsDrinkId51Unavailable}, DrinkId52Unavailable: {slaveStatus.Data13.IsDrinkId52Unavailable}, DrinkId53Unavailable: {slaveStatus.Data13.IsDrinkId53Unavailable}, DrinkId54Unavailable: {slaveStatus.Data13.IsDrinkId54Unavailable}, DrinkId55Unavailable: {slaveStatus.Data13.IsDrinkId55Unavailable}, DrinkId56Unavailable: {slaveStatus.Data13.IsDrinkId56Unavailable}");

    //        Console.WriteLine($" [Data14] DrinkId57Unavailable: {slaveStatus.Data14.IsDrinkId57Unavailable}, DrinkId58Unavailable: {slaveStatus.Data14.IsDrinkId58Unavailable}, DrinkId59Unavailable: {slaveStatus.Data14.IsDrinkId59Unavailable}, DrinkId60Unavailable: {slaveStatus.Data14.IsDrinkId60Unavailable}, DrinkId61Unavailable: {slaveStatus.Data14.IsDrinkId61Unavailable}, DrinkId62Unavailable: {slaveStatus.Data14.IsDrinkId62Unavailable}, DrinkId63Unavailable: {slaveStatus.Data14.IsDrinkId63Unavailable}, DrinkId64Unavailable: {slaveStatus.Data14.IsDrinkId64Unavailable}");

    //        Console.WriteLine($" [Data15] DrinkId65Unavailable: {slaveStatus.Data15.IsDrinkId65Unavailable}, DrinkId66Unavailable: {slaveStatus.Data15.IsDrinkId66Unavailable}, DrinkId67Unavailable: {slaveStatus.Data15.IsDrinkId67Unavailable}, DrinkId68Unavailable: {slaveStatus.Data15.IsDrinkId68Unavailable}, DrinkId69Unavailable: {slaveStatus.Data15.IsDrinkId69Unavailable}, DrinkId70Unavailable: {slaveStatus.Data15.IsDrinkId70Unavailable}, DrinkId71Unavailable: {slaveStatus.Data15.IsDrinkId71Unavailable}, DrinkId72Unavailable: {slaveStatus.Data15.IsDrinkId72Unavailable}");

    //        Console.WriteLine($" [Data16] DrinkId73Unavailable: {slaveStatus.Data16.IsDrinkId73Unavailable}, DrinkId74Unavailable: {slaveStatus.Data16.IsDrinkId74Unavailable}, DrinkId75Unavailable: {slaveStatus.Data16.IsDrinkId75Unavailable}, DrinkId76Unavailable: {slaveStatus.Data16.IsDrinkId76Unavailable}, DrinkId77Unavailable: {slaveStatus.Data16.IsDrinkId77Unavailable}, DrinkId78Unavailable: {slaveStatus.Data16.IsDrinkId78Unavailable}, DrinkId79Unavailable: {slaveStatus.Data16.IsDrinkId79Unavailable}, DrinkId80Unavailable: {slaveStatus.Data16.IsDrinkId80Unavailable}");

    //        Console.WriteLine($" [Data17] DrinkId81Unavailable: {slaveStatus.Data17.IsDrinkId81Unavailable}, DrinkId82Unavailable: {slaveStatus.Data17.IsDrinkId82Unavailable}, DrinkId83Unavailable: {slaveStatus.Data17.IsDrinkId83Unavailable}, DrinkId84Unavailable: {slaveStatus.Data17.IsDrinkId84Unavailable}, DrinkId85Unavailable: {slaveStatus.Data17.IsDrinkId85Unavailable}, DrinkId86Unavailable: {slaveStatus.Data17.IsDrinkId86Unavailable}, DrinkId87Unavailable: {slaveStatus.Data17.IsDrinkId87Unavailable}, DrinkId88Unavailable: {slaveStatus.Data17.IsDrinkId88Unavailable}");

    //        Console.WriteLine($" [Data18] DrinkId89Unavailable: {slaveStatus.Data18.IsDrinkId89Unavailable}, DrinkId90Unavailable: {slaveStatus.Data18.IsDrinkId90Unavailable}, DrinkId91Unavailable: {slaveStatus.Data18.IsDrinkId91Unavailable}, DrinkId92Unavailable: {slaveStatus.Data18.IsDrinkId92Unavailable}, DrinkId93Unavailable: {slaveStatus.Data18.IsDrinkId93Unavailable}, DrinkId94Unavailable: {slaveStatus.Data18.IsDrinkId94Unavailable}, DrinkId95Unavailable: {slaveStatus.Data18.IsDrinkId95Unavailable}, DrinkId96Unavailable: {slaveStatus.Data18.IsDrinkId96Unavailable}");

    //        Console.WriteLine($" [Data19] DrinkId89Unavailable: {slaveStatus.Data19.IsDrinkId89Unavailable}, DrinkId90Unavailable: {slaveStatus.Data19.IsDrinkId90Unavailable}, DrinkId91Unavailable: {slaveStatus.Data19.IsDrinkId91Unavailable}, DrinkId92Unavailable: {slaveStatus.Data19.IsDrinkId92Unavailable}, DrinkId93Unavailable: {slaveStatus.Data19.IsDrinkId93Unavailable}, DrinkId94Unavailable: {slaveStatus.Data19.IsDrinkId94Unavailable}, DrinkId95Unavailable: {slaveStatus.Data19.IsDrinkId95Unavailable}, DrinkId96Unavailable: {slaveStatus.Data19.IsDrinkId96Unavailable}");

    //        Console.WriteLine($" [Data20] BrewerMotorOverheated: {slaveStatus.Data20.IsBrewerMotorOverheated}, PipelineLeakage: {slaveStatus.Data20.IsPipelineLeakage}, BeanBox1PhotoelectricLow: {slaveStatus.Data20.IsBeanBox1PhotoelectricLow}, BeanBox2PhotoelectricLow: {slaveStatus.Data20.IsBeanBox2PhotoelectricLow}, BeanBox3PhotoelectricLow: {slaveStatus.Data20.IsBeanBox3PhotoelectricLow}, BeanBox1NotInstalled: {slaveStatus.Data20.IsBeanBox1NotInstalled}, BeanBox2NotInstalled: {slaveStatus.Data20.IsBeanBox2NotInstalled}, BeanBox3NotInstalled: {slaveStatus.Data20.IsBeanBox3NotInstalled}");
    //        Console.WriteLine($" [Data21] BeanBinHandleNotInPlace: {slaveStatus.Data21.IsBeanBinHandleNotInPlace}, PowderMixingBinNotInstalled: {slaveStatus.Data21.IsPowderMixingBinNotInstalled}, PhotoelectricModuleFailure: {slaveStatus.Data21.IsPhotoelectricModuleFailure}");

    //        Console.WriteLine($"Check Code: 0x{slaveStatus.CheckCode:X2}");
    //        Console.WriteLine($"End Code: 0x{slaveStatus.EndCode:X2}");
    //        Console.WriteLine("--- End of Report ---");
    //    }
    //    else
    //    {
    //        Console.WriteLine("Failed to get status.");
    //    }
    //}

    //static void MakeDrink(CoffeeMachine coffeeMachine)
    //{
    //    Console.Write("Enter Drink Type (e.g., Espresso, Latte): ");
    //    coffeeMachine.MakeDrink(14);

    //}

    //static void CleanMachine(CoffeeMachine coffeeMachine)
    //{
    //    coffeeMachine.Clean(DrinkOrCleanCommand.CommandAction.QuickMilkRinse);
    //}
}

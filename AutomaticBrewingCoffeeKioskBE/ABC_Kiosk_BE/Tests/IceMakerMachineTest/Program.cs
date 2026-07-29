﻿using System.Text;
using IceMakerMachineTest;


Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

//IceMachine iceMachineDevice = new Services.Models.DeviceConnectors.IceMachine("COM3", 115200);

//if (iceMachineDevice == null)
//{
//    Console.WriteLine("[ERROR] Thiết bị không hợp lệ.");
//    return;
//}

//Console.WriteLine($"\n[INFO] Connecting to {iceMachineDevice.GetType().Name}");
//iceMachineDevice.Connect();
Console.WriteLine("[INFO] Connection successful. Starting device...");

    Console.WriteLine("\n--- Ice Maker Control Menu ---");
    bool exit = false;
    while (!exit)
    {
        Console.WriteLine("\nChoose an action:");
        Console.WriteLine("  1. Query Status");
        Console.WriteLine("  2. Query Parameters");
        Console.WriteLine("  3. Set Parameters");
        Console.WriteLine("  4. Dispense Ice");
        Console.WriteLine("  5. Dispense Water");
        Console.WriteLine("  6. Dispense Ice-Water");
        Console.WriteLine("  7. Power Off (Z03 Only)");
        Console.WriteLine("  0. Exit");
        Console.Write("Enter choice: ");
        string menuChoice = Console.ReadLine();

        byte[] response; // To store response bytes
        bool success;    // To store success/fail boolean

        switch (menuChoice)
        {
            case "1": // Query Status
                Console.WriteLine("\n[ACTION] Sending Query Status...");
                //iceMachineDevice.QueryStatus();
                break;

            case "2": // Query Parameters
                Console.WriteLine("\n[ACTION] Sending Query Parameters...");
                //var r = iceMachineDevice.QueryParameters();
                break;
            case "3": // Set Parameters
                //iceMachineDevice.PowerOff();
                break;

            case "4": // Dispense Ice
                //iceMachineDevice.DispenseIce(
                                          //GetByteInput($"Enter Ice Quantity (0=Default, {IceMakerConstants.QuantityLimits.MinIceQuantity}-{IceMakerConstants.QuantityLimits.MaxIceQuantity}): ",
                                                       //0, IceMakerConstants.QuantityLimits.MaxIceQuantity)); // Allow 0
                break;

            case "5": // Dispense Water
                //iceMachineDevice.DispenseIce(
                                          //GetByteInput($"Enter Water Quantity (0=Default, {IceMakerConstants.QuantityLimits.MinWaterQuantity}-{IceMakerConstants.QuantityLimits.MaxWaterQuantity}): ",
                                                      // 0, IceMakerConstants.QuantityLimits.MaxWaterQuantity)); // Allow 0
                break;
            case "6": // Dispense Ice-Water
                //iceMachineDevice.DispenseIce(
                                          //GetByteInput($"Enter Ice-Water Quantity (0=Default, {IceMakerConstants.QuantityLimits.MinIceWaterQuantity}-{IceMakerConstants.QuantityLimits.MaxIceWaterQuantity}): ",
                                                       //0, IceMakerConstants.QuantityLimits.MaxIceWaterQuantity)); // Allow 0
                break;

            case "7": // Dispense Ice-Water
                //iceMachineDevice.PowerOff();
                break;
            case "0": // Exit
                exit = true;
                Console.WriteLine("\nExiting Ice Maker control menu...");
                break;

            default:
                Console.WriteLine("[ERROR] Invalid choice. Please try again.");
                break;
        }
    }


//iceMachineDevice.Disconnect(); // Assuming Disconnect handles port closing
Console.WriteLine("[INFO] Device disconnected.");
Console.WriteLine("\n=== Test Finished ===");


// --- Helper Function for Console Input ---
byte GetByteInput(string prompt, byte min, byte max)
{
    byte value;
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();
        if (byte.TryParse(input, out value) && value >= min && value <= max)
        {
            return value;
        }
        else
        {
            Console.WriteLine($"[ERROR] Invalid input. Please enter a number between {min} and {max}.");
        }
    }
}
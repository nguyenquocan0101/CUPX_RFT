using System;
using SerialDeviceConnector;
//using Services.Models.DeviceConnectors;
using Services.Utils;

class Program
{
    static void Main(string[] args)
    {
        //try
        //{
        //    // Connect to the cup dispensing machine
        //    CupDroppingMachine cupDroppingMachine = new CupDroppingMachine("COM8", 115200);
        //    cupDroppingMachine.Connect();

        //    while (true)
        //    {
        //        Console.WriteLine("\nSelect an action:");
        //        Console.WriteLine("1 - Query machine status");
        //        Console.WriteLine("2 - Dispense a cup");
        //        Console.WriteLine("q - Quit");
        //        Console.Write("Enter your choice: ");
        //        string choice = Console.ReadLine();

        //        if (choice == "q") break;

        //        switch (choice)
        //        {
        //            case "1":
        //                var slaveStatus = cupDroppingMachine.QueryStatus();
        //                if (slaveStatus != null)
        //                {
        //                    Console.WriteLine("\n--- MACHINE STATUS ---");
        //                    Console.WriteLine($"- No cup present: {slaveStatus.Data1.IsNoCup}");
        //                    Console.WriteLine($"- Cup not taken away: {slaveStatus.Data1.IsCupNotTakenAway}");
        //                    Console.WriteLine($"- Drawer pulled out: {slaveStatus.Data1.IsDrawerPulledOut}");
        //                    Console.WriteLine($"- Motor failure: {slaveStatus.Data1.IsMotorFailure}");
        //                    Console.WriteLine($"- Robot arm in place: {slaveStatus.Data1.IsRobotArmInPlace}");
        //                    Console.WriteLine($"- System status: {slaveStatus.Data2.CurrentSystemStatus}");
        //                }
        //                else
        //                {
        //                    Console.WriteLine("No status received.");
        //                }

        //                break;

        //            case "2":
        //                var dispenseStatus = cupDroppingMachine.DropOneCup();
        //                //Console.WriteLine(dispenseStatus.Result == OperationResult.Success
        //                //    ? "Dispense cup command sent."
        //                //    : "Failed to send dispense cup command.");
        //                break;

        //            default:
        //                Console.WriteLine("Invalid choice.");
        //                break;
        //        }
        //    }

        //    // Disconnect
        //    cupDroppingMachine.Disconnect();
        //    Console.WriteLine("Disconnected from the cup dispensing machine.");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine("Error: " + ex.Message);
        //}
    }
}
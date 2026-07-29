using SerialDeviceConnector;
using System;
using System.Linq;

internal class SerialResponseParser
{
    private const byte EndCode = 0xFF; // End marker for all commands
    /// <summary>
    /// Parses a received byte array and extracts meaningful data.
    /// </summary>
    /// <param name="response">The raw byte array received from the device.</param>
    /// <returns>A structured response object.</returns>
    public static ParsedResponse Parse(byte[] response)
    {
        if (response == null || response.Length < 4)
        {
            throw new ArgumentException("Invalid response data.");
        }

        // Extract components from response
        byte commandCode = response[0];
        byte lengthCode = response[1];
        byte instructionCode = response[2];
        byte checksum = response[response.Length - 2];
        byte endCode = response[response.Length - 1];

        // Validate end code
        if (endCode != EndCode)
        {
            throw new InvalidOperationException("Invalid end code.");
        }

        // Extract data bytes (excluding command, length, instruction, checksum, and end code)
        //byte[] data = response.Skip(3).Take(response.Length - 5).ToArray();
        byte[] data = response.ToArray();
        // Validate checksum
        if (!ValidateChecksum(response, checksum))
        {
            throw new InvalidOperationException("Checksum validation failed.");
        }

        return new ParsedResponse(commandCode, lengthCode, instructionCode, data);
    }

    /// <summary>
    /// Validates the checksum of the received data.
    /// </summary>
    private static bool ValidateChecksum(byte[] response, byte expectedChecksum)
    {
        byte calculatedChecksum = (byte)(response.Take(response.Length - 2).Sum(b => b) & EndCode);
        return calculatedChecksum == expectedChecksum;
    }
}

/// <summary>
/// Represents a structured response after parsing.
/// </summary>
public class ParsedResponse
{
    public byte CommandCode { get; }
    public byte LengthCode { get; }
    public byte InstructionCode { get; }
    public byte[] Data { get; }

    public ParsedResponse(byte commandCode, byte lengthCode, byte instructionCode, byte[] data)
    {
        CommandCode = commandCode;
        LengthCode = lengthCode;
        InstructionCode = instructionCode;
        Data = data;
    }
}

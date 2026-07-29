namespace SerialDeviceConnector
{
    public class CommandBuilder
    {
        private readonly List<byte> _command;
        private const byte EndCode = 0xFF; // End marker for all commands

        public CommandBuilder()
        {
            _command = new List<byte>();
        }

        /// <summary>
        /// Add Command Code
        /// </summary>
        public CommandBuilder AddCommandCode(byte commandCode)
        {
            _command.Add(commandCode);
            return this;
        }

        /// <summary>
        /// Add Instruction Code
        /// </summary>
        public CommandBuilder AddInstructionCode(byte instructionCode)
        {
            _command.Add(instructionCode);
            return this;
        }

        /// <summary>
        /// Add data query (at least 1 data params)
        /// </summary>
        public CommandBuilder AddData(params byte[] data)
        {
            _command.AddRange(data);
            return this;
        }

        /// <summary>
        /// Calculate Length Code base on bytes count
        /// LengthCode = "Command Code" + "Length Code" + "Instruction Code" + "Data 1" + "Data ... n" + "Checksum Code" + "End Code"
        /// </summary>
        public byte CalculateLengthCode()
        {
            return (byte)(_command.Count + 3); // +3 includes Length Code & Checksum & End Code
        }

        /// <summary>
        ///Calculate Checksum Code
        ///Checksum = "Command Code" + "Length Code" + "Instruction Code" + "Data 1" + "Data 2" + ... + "Data n"
        /// </summary>
        private byte CalculateChecksum(List<byte> finalCommand)
        {
            return (byte)(finalCommand.Sum(b => b) & EndCode); // Sum all bytes and take the last 8 bits
        }

        /// <summary>
        /// Build Command
        /// Formular: "Command Code" + "Length Code" + "Instruction Code" + "Data 1...n" + "Checksum Code" + "End Code"
        /// </summary>
        public byte[] Build()
        {
            List<byte> finalCommand = new List<byte>();

            // Add Command Code and Length Code (auto-calculated)
            finalCommand.Add(_command[0]);
            finalCommand.Add(CalculateLengthCode());

            // Add Instruction Code & Data
            finalCommand.AddRange(_command.Skip(1));

            // Add Checksum Code
            finalCommand.Add(CalculateChecksum(finalCommand));

            // Add End Code 
            finalCommand.Add(EndCode);
            //clear command
            _command.Clear();
            return finalCommand.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
// Assuming the encoder classes are in the InstructionEncoder namespace:
using InstructionEncoder;

namespace VMAssembler
{
    /// <summary>
    /// Stores label information: the source line number and the memory location.
    /// </summary>
    public class LabelInfo
    {
        public int LineNumber { get; }
        public int MemoryLocation { get; }

        public LabelInfo(int lineNumber, int memoryLocation)
        {
            LineNumber = lineNumber;
            MemoryLocation = memoryLocation;
        }
    }

    // Fallback BasicInstruction in case an instruction isn’t recognized.
    // (If encoder.cs already provides one, you may remove this.)
    public class BasicInstruction : IInstruction
    {
        private readonly string _instructionText;
        public BasicInstruction(string instructionText)
        {
            _instructionText = instructionText;
        }
        public int Encode()
        {
            // For unrecognized instructions, simply return 0.
            return 0;
        }
    }

    class Program
    {
        // The magic header bytes.
        private static readonly byte[] MagicHeader = { 0xde, 0xad, 0xbe, 0xef };

        static void Main(string[] args)
        {
            // Get input file path (assembly source). Default is "asm/input.asm".
            string filePath = args.Length > 0 ? args[0] : Path.Combine("asm", "input.asm");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            // Read all lines from the assembly source file.
            string[] lines = File.ReadAllLines(filePath);

            // ----------------------------
            // PASS 1: Label recording
            // ----------------------------
            Dictionary<string, LabelInfo> labels = new Dictionary<string, LabelInfo>();
            int lineNumber = 1;    // Source file line counter.
            int memLocation = 0;   // Memory location (in bytes; increments by 4 per machine instruction).

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    lineNumber++;
                    continue;
                }

                // Remove inline comments.
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex).Trim();

                if (string.IsNullOrEmpty(line))
                {
                    lineNumber++;
                    continue;
                }

                // If the line is a label (ends with a colon).
                if (line.EndsWith(":"))
                {
                    string labelName = line.Substring(0, line.Length - 1).Trim();
                    if (!labels.ContainsKey(labelName))
                    {
                        labels.Add(labelName, new LabelInfo(lineNumber, memLocation));
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Duplicate label '{labelName}' found on line {lineNumber}.");
                    }
                }
                else
                {
                    // For instructions, determine the expansion count.
                    int count = GetInstructionCount(line);
                    memLocation += count * 4;
                }
                lineNumber++;
            }

            // For debugging, display recorded labels.
            Console.WriteLine("Labels Recorded:");
            foreach (var kvp in labels)
            {
                Console.WriteLine($"Label: {kvp.Key} - Source Line: {kvp.Value.LineNumber}, Memory Location: {kvp.Value.MemoryLocation}");
            }

            // ----------------------------
            // PASS 2: Instruction encoding
            // ----------------------------
            List<IInstruction> instructionList = new List<IInstruction>();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex).Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                // Skip label lines.
                if (line.EndsWith(":"))
                    continue;

                // Split the line into tokens.
                string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                    continue;

                string opcode = tokens[0].ToLower();
                switch (opcode)
                {
                    case "stpush":
                        {
                            // Everything after the opcode is the parameter.
                            string parameter = line.Substring(tokens[0].Length).Trim();
                            // Use the new encoder's StPushExpander to get a list of push instructions.
                            List<PushInstruction> pushes = StPushExpander.Expand(parameter);
                            instructionList.AddRange(pushes);
                            break;
                        }
                    case "dup":
                        {
                            // Use DupInstruction from the new encoder.
                            int offset = 0;
                            if (tokens.Length >= 2)
                            {
                                if (!int.TryParse(tokens[1], out offset))
                                {
                                    Console.WriteLine($"Error: Invalid parameter for dup instruction: {line}");
                                    return;
                                }
                            }
                            instructionList.Add(new DupInstruction(offset));
                            break;
                        }
                    case "exit":
                        {
                            int code = 0;
                            if (tokens.Length >= 2)
                            {
                                if (!int.TryParse(tokens[1], out code))
                                {
                                    Console.WriteLine($"Error: Invalid parameter for exit instruction: {line}");
                                    return;
                                }
                            }
                            instructionList.Add(new ExitInstruction(code));
                            break;
                        }
                    case "swap":
                        {
                            // Defaults: from=4, to=0.
                            int from = 4, to = 0;
                            if (tokens.Length >= 2)
                            {
                                if (!int.TryParse(tokens[1], out from))
                                {
                                    Console.WriteLine($"Error: Invalid 'from' parameter for swap instruction: {line}");
                                    return;
                                }
                            }
                            if (tokens.Length >= 3)
                            {
                                if (!int.TryParse(tokens[2], out to))
                                {
                                    Console.WriteLine($"Error: Invalid 'to' parameter for swap instruction: {line}");
                                    return;
                                }
                            }
                            instructionList.Add(new SwapInstruction(from, to));
                            break;
                        }
                    case "nop":
                        {
                            instructionList.Add(new Nop());
                            break;
                        }
                    case "input":
                        {
                            instructionList.Add(new InputInstruction());
                            break;
                        }
                    case "stinput":
                        {
                            if (tokens.Length < 2)
                            {
                                Console.WriteLine($"Error: stinput requires a value: {line}");
                                return;
                            }

                            string valueToken = tokens[1];
                            NumberStyles style = valueToken.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                                ? NumberStyles.HexNumber
                                : NumberStyles.Integer;

                            if (valueToken.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                            {
                                valueToken = valueToken.Substring(2); // Remove "0x"
                            }

                            if (!int.TryParse(valueToken, style, CultureInfo.InvariantCulture, out int value))
                            {
                                Console.WriteLine($"Error: Invalid value for stinput instruction: {line}");
                                return;
                            }

                            instructionList.Add(new StInputInstruction(value));
                            break;
                        }
                    case "debug":
                        {
                            // In our new encoder, DebugInstruction takes an optional value.
                            // Here we check for a value; if none is provided, default to 0.
                            int debugValue = 0;

                            if (tokens.Length >= 2)
                            {
                                string valueToken = tokens[1];

                                // Check if hex mode is used
                                bool isHex = valueToken.ToLower() == "hex";

                                if (isHex)
                                {
                                    if (tokens.Length >= 3)
                                    {
                                        valueToken = tokens[2];
                                        if (!int.TryParse(valueToken, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out debugValue))
                                        {
                                            Console.WriteLine($"Error: Invalid hex value for debug instruction: {line}");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!int.TryParse(valueToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out debugValue))
                                    {
                                        Console.WriteLine($"Error: Invalid decimal value for debug instruction: {line}");
                                        return;
                                    }
                                }
                            }

                            // Our DebugInstruction in the encoder currently accepts the value.
                            instructionList.Add(new DebugInstruction(debugValue));
                            break;
                        }
                    default:
                        {
                            // For any unrecognized instruction, add as a basic placeholder.
                            instructionList.Add(new BasicInstruction(line));
                            break;
                        }
                }
            }

            // Error if no instructions are assembled.
            if (instructionList.Count == 0)
            {
                Console.WriteLine("Error: No instructions to assemble.");
                return;
            }

            // Pad the instruction list with nop instructions until the total is a multiple of 4.
            while (instructionList.Count % 4 != 0)
            {
                instructionList.Add(new Nop());
            }

            // ----------------------------
            // Write binary output file.
            // ----------------------------
            string outputFile = "output.bin";
            using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (BinaryWriter binWriter = new BinaryWriter(fs))
            {
                // Write the magic header.
                binWriter.Write(MagicHeader);

                // Write each instruction's 4-byte encoding in little-endian format.
                foreach (var inst in instructionList)
                {
                    int encoded = inst.Encode();
                    binWriter.Write(encoded);
                }
            }

            Console.WriteLine($"Assembly successful. {instructionList.Count} instructions written to {outputFile}");
        }

        /// <summary>
        /// Determines how many machine instructions a given line expands into.
        /// For most instructions this is 1, but for stpush it depends on the string length.
        /// </summary>
        /// <param name="line">A trimmed assembly line (comments removed)</param>
        /// <returns>Number of machine instructions</returns>
        static int GetInstructionCount(string line)
        {
            string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return 0;

            string opcode = tokens[0].ToLower();
            if (opcode == "stpush")
            {
                string parameter = line.Substring(tokens[0].Length).Trim();
                if (parameter.StartsWith("\"") && parameter.EndsWith("\"") && parameter.Length >= 2)
                    parameter = parameter.Substring(1, parameter.Length - 2);
                // stpush expands into push instructions in groups of 3 characters.
                int chunks = (parameter.Length + 2) / 3;
                return chunks;
            }
            // All other instructions count as one machine instruction.
            return 1;
        }
    }
}

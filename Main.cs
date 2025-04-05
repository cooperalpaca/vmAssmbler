class Assembler
{
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            System.Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} <file.asm> <file.v>");
            Environment.Exit(1);
        }

        string inputFile = args[0];
        string outputFile = args[1];

        Dictionary<string, int> labelMap = new Dictionary<string, int>();
        List<AssemblerLine> source = new List<AssemblerLine>();
        int dummyPC = 0;

        try
        {
            using (StreamReader sr = new StreamReader(inputFile, System.Text.Encoding.ASCII))
            {
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    AssemblerLine[]? instructions;
                    bool label;
                    try {
                        (instructions, label) = FirstPass.ProcessOneLine(line.Trim());
                    } catch (Exception e) {
                        System.Console.WriteLine(e.Message);
                        return;
                    }
                    if (instructions == null || instructions.Length == 0)
                    {
                        // The line is likely a comment or just blank
                        continue;
                    }

                    if (label)
                    {
                        // A label, just use the existing PC
                        labelMap.Add(instructions[0].OriginalLine, dummyPC);
                        continue;
                    }

                    // If we get here we have 1 or more valid instructions to add
                    dummyPC += instructions.Length * 4;
                    source.AddRange(instructions);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        // First pass should be done now
        // Now we can go through the lines and make them into instructions
        List<Instruction.IInstruction> instructionsList = new List<Instruction.IInstruction>();
        // Regular for-loop because we need `i` to keep track of the program counter
        // You'll need to for branch/goto instructions
        if (source.Count() < 1)
        {
            Console.WriteLine("ERROR: " + inputFile + ": no instructions to assemble.");
            return;
        }
        for (int i = 0; i < source.Count(); i++)
        {
            // In some cases, first argument can be a label
            string? argOneStr = source[i].Tokens.ElementAtOrDefault(1);
            // Try parsing arguments as integers
            //
            int? argOne = null;
            int? argTwo = null; try
            {
                argOne = Utils.ParseInt(argOneStr);
                argTwo = Utils.ParseInt(source[i].Tokens.ElementAtOrDefault(2));
            }
            catch (Exception)
            {
                return;
            }
            // First argument is a string, try parsing a label
            int? offset = null;
            int labelAddress = -1;
            if (argOneStr != null && !argOne.HasValue)
            {
                if (labelMap.TryGetValue(argOneStr, out labelAddress))
                {
                    int currAddress = i * 4;
                    offset = labelAddress - currAddress;
                }
                else
                {
                    System.Console.WriteLine($"Invalid label: The given key '{argOneStr}' was not present in the dictionary.");
                }
            }
            Instruction.IInstruction ins;
            try {
            ins = source[i].Tokens[0].ToLower() switch
            {
                "exit" => new Exit(argOne),
                "swap" => new Swap(argOne, argTwo),
                "nop" => new Nop(),
                "input" => new Input(),
                "stinput" => new StInput(argOne),
                "debug" => new Debug(argOne),
                "pop" => new Pop(argOne),
                "add" => new Add(),
                "sub" => new Sub(),
                "mul" => new Mul(),
                "div" => new Div(),
                "rem" => new Rem(),
                "and" => new And(),
                "or" => new Or(),
                "xor" => new Xor(),
                "lsl" => new Lsl(),
                "lsr" => new Lsr(),
                "asr" => new Asr(),
                "neg" => new Neg(),
                "not" => new Not(),
                "stprint" => new Stprint(argOne),
                "call" => new Call(offset),
                "return" => new Return(argOne),
                "goto" => new Goto(offset),
                "ifeq" => new Ifeq(offset),
                "ifne" => new Ifne(offset),
                "iflt" => new Iflt(offset),
                "ifgt" => new Ifgt(offset),
                "ifle" => new Ifle(offset),
                "ifge" => new Ifge(offset),
                "ifez" => new Ifez(offset),
                "ifnz" => new Ifnz(offset),
                "ifmi" => new Ifmi(offset),
                "ifpl" => new Ifpl(offset),
                "dup" => new Dup(argOne),
                "print" => new Print(argOne, 'd'),
                "printh" => new Print(argOne, 'h'),
                "printo" => new Print(argOne, 'o'),
                "printb" => new Print(argOne, 'b'),
                "dump" => new Dump(),
                "push" => new Push(argOne),
                _ => throw new Exception($"Unknown instruction: {source[i].Tokens[0].ToLower()}"),
            };
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
                return;
            }

            instructionsList.Add(ins);
        }

        using (var outStream = File.Open(outputFile, FileMode.Create))
        {
            using (BinaryWriter br = new BinaryWriter(outStream))
            {
                // Magic Header (Backwards b/c little endian)
                UInt32 deadBeef = 0xEFBE_ADDE;
                br.Write(deadBeef);

                // All remaining instructions
                foreach (var v in instructionsList)
                {
                    br.Write(v.Encode());
                }

                // Pad out to a multiple of 4 instructions
                if (instructionsList.Count() % 4 != 0)
                {
                    int n = new Nop().Encode();
                    for (int i = 0; i < 4 - (instructionsList.Count() % 4); i++)
                    {
                        br.Write(n);
                    }
                }
            }
        }
    }
}

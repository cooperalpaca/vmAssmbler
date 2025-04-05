public class FirstPass
{
    // `ProcessOneLine` takes in a single line of assembly input and returns one or more
    // instructions. If the return bool is true the returned line is a label which is handled differently.
    public static (AssemblerLine[]?, bool) ProcessOneLine(string line)
    {
        List<AssemblerLine> instructions = new List<AssemblerLine>();
        // Empty Line
        if (line.Length == 0)
        {
            return (null, false);
        }

        // Line begins with a comment
        if (line[0] == '#')
        {
            return (null, false);
        }

        // Split the entire line by whitespace, then we can determine if it is a label or not
        string[] tokens;
        try
        {
            tokens = Utils.TokenizeString(line);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }

        // A label ends with ':'
        if (tokens[0].Last() == ':')
        {
            // Add the label, removing the final character (':')
            instructions.Add(new(tokens[0].Substring(0, tokens[0].Length - 1), new string[0]));
            return (instructions.ToArray(), true);
        }

        // Before we can add the instruction we need to check if it is a pseudoinstruciton
        // In this case I'm just going to hardcode this as there is only one
        if (tokens[0].Equals("stpush"))
        {
            string workingCopy = tokens[1];
            // Padding out the string with 0's because I'm lazy
            // and this makes life a lot easier
            workingCopy = workingCopy.PadRight((workingCopy.Length % 3 == 0) ?
                                                   0 :
                                                   workingCopy.Length + 3 - (workingCopy.Length % 3), (char)1);

            if (workingCopy.Length % 3 != 0)
            {
                throw new Exception($"Wrong string length (was {tokens[1].Length}, then {workingCopy.Length})");
            }

            // It takes 3 lines to reverse a string?
            char[] tmp = workingCopy.ToCharArray();
            Array.Reverse(tmp);
            string reversedCopy = new string(tmp);

            // Break the string apart into 3 byte chunks, then push those
            // all as separate "lines"
            int pushCount = workingCopy.Length / 3;
            for (int i = 0; i < pushCount; i++)
            {
                Int32 b = (reversedCopy[i * 3] << 16) | (reversedCopy[i * 3 + 1] << 8) | (reversedCopy[i * 3 + 2]);

                if (i != 0)
                {
                    b |= (0x1 << 24);
                }

                // I am adding the inline comments from the writeup to aid debugging
                string[] pseudoLine = { "push",
                                                $"0x{b:x8}",
                                                "#", Utils.PrettyComment(reversedCopy[i*3+2], reversedCopy[i*3+1], reversedCopy[i*3]),
                                                (i != 0 ? "(cont)" : "(stop)")};
                AssemblerLine asmLine = new(line.Trim() + $" ({i + 1}/{pushCount})", pseudoLine);
                instructions.Add(asmLine);
            }

            return (instructions.ToArray(), false);
        }

        // If we make it here we either have an error (not yet handled)
        // Or we think we have a valid instruction
        AssemblerLine al = new(line.Trim(), tokens);
        instructions.Add(al);
        return (instructions.ToArray(), false);
    }
}

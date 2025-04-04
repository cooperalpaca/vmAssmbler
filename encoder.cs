using System;
using System.Collections.Generic;
using System.Globalization;

namespace InstructionEncoder
{
    public interface IInstruction
    {
        /// <summary>
        /// Returns the 32-bit encoded representation of the instruction.
        /// </summary>
        int Encode();
    }

    // ---------------------------
    // Miscellaneous Instructions (opcode=0)
    // ---------------------------

    // Exit Instruction
    // Encoding: [31:28]=0, [27:24]=0x1, [23:0]=exit code (default 0)
    public class ExitInstruction : IInstruction
    {
        private readonly int _code;
        public ExitInstruction(int code = 0)
        {
            _code = code;
        }
        public int Encode()
        {
            return (0x0 << 28) | (0x1 << 24) | (_code & 0xFFFFFF);
        }
    }

    // Swap Instruction
    // Encoding: [31:28]=0, [27:24]=0x2, [23:12]=from (12 bits, offset>>2), [11:0]=to (12 bits, offset>>2)
    public class SwapInstruction : IInstruction
    {
        private readonly int _from;
        private readonly int _to;
        public SwapInstruction(int from = 4, int to = 0)
        {
            _from = from;
            _to = to;
        }
        public int Encode()
        {
            int encodedFrom = ((_from >> 2) & 0xFFF);
            int encodedTo = ((_to >> 2) & 0xFFF);
            return (0x0 << 28) | (0x2 << 24) | (encodedFrom << 12) | encodedTo;
        }
    }

    // NOP Instruction
    // Encoding: [31:28]=0, [27:24]=0x4, remainder 0.
    public class Nop : IInstruction
    {
        public int Encode()
        {
            return (0x0 << 28) | (0x4 << 24);
        }
    }

    // Input Instruction
    // Encoding: [31:28]=0, [27:24]=0x5
    public class InputInstruction : IInstruction
    {
        public int Encode()
        {
            return (0x0 << 28) | (0x5 << 24);
        }
    }

    // stinput Instruction
    // Encoding: [31:28]=0, [27:24]=0xF, [23:0]=max characters (if omitted, default is all ones: 0x00FFFFFF)
    public class StInputInstruction : IInstruction
    {
        private readonly int _maxChars;
        public StInputInstruction(int maxChars = 0x00FFFFFF)
        {
            _maxChars = maxChars;
        }
        public int Encode()
        {
            return (0x0 << 28) | (0xF << 24) | (_maxChars & 0xFFFFFF);
        }
    }

    // Debug Instruction
    // Encoding: [31:28]=0, [27:24]=0xD, [23:0]=optional value (default 0)
    public class DebugInstruction : IInstruction
    {
        private readonly int _value;
        public DebugInstruction(int value = 0)
        {
            _value = value;
        }
        public int Encode()
        {
            return (0x0 << 28) | (0xD << 24) | (_value & 0xFFFFFF);
        }
    }

    // ---------------------------
    // Pop Instruction (opcode=1)
    // ---------------------------
    // pop [unsigned offset]
    // Encoding: [31:28]=1, [27:0]=unsigned offset (default 4)
    public class PopInstruction : IInstruction
    {
        private readonly int _offset;
        public PopInstruction(int offset = 4)
        {
            _offset = offset;
        }
        public int Encode()
        {
            return (0x1 << 28) | (_offset & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // Binary Arithmetic Instructions (opcode=2)
    // ---------------------------
    // We define a base class for binary arithmetic instructions.
    public abstract class BinaryArithmeticInstruction : IInstruction
    {
        protected abstract int Subopcode { get; }
        public int Encode()
        {
            // Bits [31:28] = 0x2, bits [27:24] = subopcode, rest zero.
            return (0x2 << 28) | (Subopcode << 24);
        }
    }

    public class AddInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x1; }
    public class SubInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x2; }
    public class MulInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x3; }
    public class DivInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x4; }
    public class RemInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x5; }
    public class AndInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x6; }
    public class OrInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x7; }
    public class XorInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x8; }
    public class LslInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0x9; }
    public class AsrInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0xA; }
    public class LsrInstruction : BinaryArithmeticInstruction { protected override int Subopcode => 0xB; }

    // ---------------------------
    // Unary Arithmetic Instructions (opcode=3)
    // ---------------------------
    public abstract class UnaryArithmeticInstruction : IInstruction
    {
        protected abstract int Subopcode { get; }
        public int Encode()
        {
            // Bits [31:28]=0x3, bits [27:24]=subopcode.
            return (0x3 << 28) | (Subopcode << 24);
        }
    }

    public class NegInstruction : UnaryArithmeticInstruction { protected override int Subopcode => 0x0; }
    public class NotInstruction : UnaryArithmeticInstruction { protected override int Subopcode => 0x1; }

    // ---------------------------
    // String Print Instruction (opcode=4)
    // ---------------------------
    // stprint [dec|hex] [offset]
    // For this example, we encode:
    // Bits [31:28]=0x4, next 12 bits: stack-relative offset (offset>>2),
    // lower 2 bits: format (00=dec, 01=hex, 10=binary, 11=octal).
    public class StPrintInstruction : IInstruction
    {
        private readonly int _offset;
        private readonly int _format; // assume values 0,1,2,3.
        public StPrintInstruction(int offset = 0, int format = 0)
        {
            _offset = offset;
            _format = format;
        }
        public int Encode()
        {
            int encodedOffset = ((_offset >> 2) & 0xFFF);
            // Place encodedOffset in bits [27:16] and format in bits [1:0]
            return (0x4 << 28) | (encodedOffset << 4) | (_format & 0x3);
        }
    }

    // ---------------------------
    // Call Instruction (opcode=5)
    // ---------------------------
    // call <label>  (PC-relative offset)
    public class CallInstruction : IInstruction
    {
        private readonly int _offset;
        public CallInstruction(int offset)
        {
            // Assume offset is already a multiple of 4.
            _offset = offset;
        }
        public int Encode()
        {
            return (0x5 << 28) | (_offset & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // Return Instruction (opcode=6)
    // ---------------------------
    // return [offset]
    public class ReturnInstruction : IInstruction
    {
        private readonly int _offset;
        public ReturnInstruction(int offset = 0)
        {
            _offset = offset;
        }
        public int Encode()
        {
            return (0x6 << 28) | (_offset & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // Goto Instruction (opcode=7)
    // ---------------------------
    // goto <label>  (PC-relative offset)
    public class GotoInstruction : IInstruction
    {
        private readonly int _offset;
        public GotoInstruction(int offset)
        {
            _offset = offset;
        }
        public int Encode()
        {
            return (0x7 << 28) | (_offset & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // Binary If Instruction (opcode=8)
    // ---------------------------
    // if<cond> <label>
    // Bits [31:28]=0x8, bits [27:25]=condition (3 bits), bits [24:0]=PC relative offset.
    public class BinaryIfInstruction : IInstruction
    {
        private readonly int _condition; // 0: eq, 1: ne, 2: lt, 3: gt, 4: le, 5: ge.
        private readonly int _offset;
        public BinaryIfInstruction(int condition, int offset)
        {
            _condition = condition & 0x7;
            _offset = offset;
        }
        public int Encode()
        {
            return (0x8 << 28) | (_condition << 25) | (_offset & 0x1FFFFFF);
        }
    }

    // ---------------------------
    // Unary If Instruction (opcode=9)
    // ---------------------------
    // if<cond> <label>
    // Bits [31:28]=0x9, bits [27:26]=condition (2 bits), bits [25:0]=PC relative offset.
    public class UnaryIfInstruction : IInstruction
    {
        private readonly int _condition; // 0: ez, 1: nz, 2: mi, 3: pl.
        private readonly int _offset;
        public UnaryIfInstruction(int condition, int offset)
        {
            _condition = condition & 0x3;
            _offset = offset;
        }
        public int Encode()
        {
            return (0x9 << 28) | (_condition << 26) | (_offset & 0x3FFFFFF);
        }
    }

    // ---------------------------
    // Dup Instruction (opcode=12)
    // ---------------------------
    // dup [offset]
    // Encoding: [31:28]=0xC, lower bits store (offset>>2) (assume full immediate in lower 28 bits).
    public class DupInstruction : IInstruction
    {
        private readonly int _offset;
        public DupInstruction(int offset = 0)
        {
            _offset = offset;
        }
        public int Encode()
        {
            return (0xC << 28) | ((_offset >> 2) & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // Print Instruction (opcode=13)
    // ---------------------------
    // print[h|o|b] [offset]
    // Encoding: [31:28]=0xD, next 12 bits: (offset>>2), lower 2 bits: format (00=dec,01=hex,10=binary,11=octal)
    public class PrintInstruction : IInstruction
    {
        private readonly int _offset;
        private readonly int _format;
        public PrintInstruction(int offset = 0, int format = 0)
        {
            _offset = offset;
            _format = format;
        }
        public int Encode()
        {
            int encodedOffset = ((_offset >> 2) & 0xFFF);
            return (0xD << 28) | (encodedOffset << 2) | (_format & 0x3);
        }
    }

    // ---------------------------
    // Dump Instruction (opcode=14)
    // ---------------------------
    // dump
    // Encoding: [31:28]=0xE, remainder zero.
    public class DumpInstruction : IInstruction
    {
        public int Encode()
        {
            return (0xE << 28);
        }
    }

    // ---------------------------
    // Push Instruction (opcode=15)
    // ---------------------------
    // push [dec|hex|label]
    // Encoding: [31:28]=0xF, [27:0]=value to push.
    public class PushInstruction : IInstruction
    {
        private readonly int _value;
        public PushInstruction(int value)
        {
            _value = value;
        }
        public int Encode()
        {
            return (0xF << 28) | (_value & 0x0FFFFFFF);
        }
    }

    // ---------------------------
    // String Push Pseudo-instruction (stpush)
    // ---------------------------
    // Expands into multiple push instructions.
    // Each push pushes 3 characters plus a flag byte.
    // The flag: 0x1 for continuation, 0x0 for final chunk.
    public static class StPushExpander
    {
        public static List<PushInstruction> Expand(string input)
        {
            // Process escape sequences (\\, \n, \")
            input = input.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");

            List<PushInstruction> pushes = new List<PushInstruction>();

            // We'll break the string into groups of 3 characters.
            int len = input.Length;
            int numChunks = (len + 2) / 3; // ceiling division

            // The expansion order is reversed: last push encodes the first group to be printed.
            for (int chunk = numChunks - 1; chunk >= 0; chunk--)
            {
                int start = chunk * 3;
                int count = Math.Min(3, len - start);
                // Read the 3 characters in order (lowest-order byte is first character)
                byte b0 = count > 0 ? (byte)input[start] : (byte)0;
                byte b1 = count > 1 ? (byte)input[start + 1] : (byte)0;
                byte b2 = count > 2 ? (byte)input[start + 2] : (byte)0;
                // Flag: if this is the last chunk (i.e. first printed) then termination flag = 0; otherwise continuation flag = 1.
                int flag = (chunk == numChunks - 1) ? 0x0 : 0x1;
                // Build a 28-bit immediate: flag in top 4 bits, then 3 bytes (24 bits)
                int immediate = ((flag & 0xF) << 24) | ((b2 & 0xFF) << 16) | ((b1 & 0xFF) << 8) | (b0 & 0xFF);
                // Ensure immediate fits in 28 bits (it will if flag is 0 or 1).
                pushes.Add(new PushInstruction(immediate));
            }
            return pushes;
        }
    }


}

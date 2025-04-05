using System.Globalization;

public class Ifez : Instruction.IInstruction
{
    private readonly int encodedOffset = 0;

    public Ifez(int? offset)
    {
        if (offset != null)
        {
            encodedOffset = (int)offset;
        }
    }
    public int Encode()
    {
        return (0b1001 << 28) | (encodedOffset & 0x1FFFFFF);
    }
}

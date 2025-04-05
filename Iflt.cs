public class Iflt : Instruction.IInstruction
{
    private readonly int encodedOffset = 0;

    public Iflt(int? offset)
    {
        if (offset != null)
        {
            encodedOffset = (int)offset;
        }
    }
    public int Encode()
    {
        return (0b1000 << 28) |  (0b010 << 25) | (encodedOffset  & 0x1FFFFFF);
    }
}

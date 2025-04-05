public class Dup : Instruction.IInstruction
{
    private readonly int _offset = 0;

    public Dup(int? offset)
    {
        if (offset != null)
        {
            _offset = (int)offset & ~3;
        }
    }

    public int Encode()
    {
        return (0b1100 << 28) | (_offset & 0x0FFFFFFF);
    }
}

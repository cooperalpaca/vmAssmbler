public class Call : Instruction.IInstruction
{
    readonly int _offset = 0;
    public Call(int? offset)
    {
        if (offset != null)
        {
            _offset = (int)offset & ~3;
        }
    }

    public int Encode()
    {
        return (0b0101 << 28) | (_offset & 0x0FFFFFFF);
    }
}

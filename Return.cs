public class Return : Instruction.IInstruction
{
    private readonly int _offset = 0;

    public Return(int? offset)
    {
        if (offset != null)
        {
            _offset = (int)offset & ~3;
        }
    }

    public int Encode()
    {
        return (0b0110 << 28) | _offset;
    }
}

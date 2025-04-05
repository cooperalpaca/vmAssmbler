public class Pop : Instruction.IInstruction
{
    private readonly int _offset = 4;

    public Pop(int? offset)
    {
        if (offset is int o)
        {
            _offset = o;
        }
    }

    public int Encode()
    {
        return (0b0001 << 28) | (_offset & ~3);
    }
}

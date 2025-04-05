public class Goto : Instruction.IInstruction
{
    private readonly int _offset = 0;

    public Goto(int? offset)
    {
        if (offset != null)
        {
            _offset = (int)offset;
        }
    }

    public int Encode()
    {
        return (0b0111 << 28) | (_offset & 0x0fffffff);
    }
}

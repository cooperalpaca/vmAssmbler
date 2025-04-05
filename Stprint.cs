public class Stprint : Instruction.IInstruction
{
    private readonly int _offset = 0;

    public Stprint(int? offset)
    {
        if (offset != null)
        {
            _offset = (int)offset;
        }
    }

    public int Encode()
    {
        return (0b0100 << 28) | (_offset & 0x0fffffff);
    }
}

public class Debug : Instruction.IInstruction
{
    private readonly int _value = 0;

    public Debug(int? val)
    {
        if (val is int v)
        {
            _value = v;
        }
    }

    public int Encode()
    {
        return (0b1111 << 24) | (_value & 0xffffff);
    }
}

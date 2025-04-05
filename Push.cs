public class Push : Instruction.IInstruction
{
    private readonly int _value = 0;

    public Push(int? value)
    {
        if (value != null)
        {
            _value = (int)value;
        }
    }

    public int Encode()
    {
        return (0b1111 << 28) | _value;
    }
}
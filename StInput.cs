public class StInput : Instruction.IInstruction
{
    private readonly int _maxChars = 0xffffff;

    public StInput(int? maxChars)
    {
        if (maxChars is int mc)
        {
            _maxChars = mc;
        }
    }

    public int Encode()
    {
        return (0b0101 << 24) | (_maxChars & 0xffffff);
    }
}

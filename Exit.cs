public class Exit : Instruction.IInstruction
{
    private readonly int _code = 0;

    public Exit(int? code)
    {
        if (code != null)
        {
            _code = (int)code;
        }
    }

    public int Encode()
    {
        return _code & 0xff;
    }
}

public class Print : Instruction.IInstruction
{
    private readonly int _offset = 0;
    private readonly int _format = 0;

    public Print(int? offset, char mode)
    {
        if (offset != null)
        {
            _offset = (int)offset;
        }
        switch(mode)
        {
            case 'h':
                _format = 0b01;
                break;
            case 'b':
                _format = 0b10;
                break;
            case 'o':
                _format = 0b11;
                break;
            default:
                break;
        }
    }

    public int Encode()
    {
        return (0b1101 << 28) | (_offset & 0x0ffffffc) | _format;
    }
}

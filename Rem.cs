public class Rem : Instruction.IInstruction
{
    public Rem() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b0100 << 24);
    }
}

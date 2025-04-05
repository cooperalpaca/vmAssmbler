public class And : Instruction.IInstruction
{
    public And() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b0101 << 24);
    }
}

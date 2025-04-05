public class Div : Instruction.IInstruction
{
    public Div() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b0011 << 24);
    }
}

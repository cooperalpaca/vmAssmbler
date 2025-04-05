public class Nop : Instruction.IInstruction
{
    public Nop() { }

    public int Encode()
    {
        return (0b0010 << 24);
    }
}

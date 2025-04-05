public class Mul : Instruction.IInstruction
{
    public Mul() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b0010 << 24);
    }
}

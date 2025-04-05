public class Neg : Instruction.IInstruction {
    public Neg() { }

    public int Encode()
    {
        return (0b0011 << 28) | (0b0000 << 24);
    }
}

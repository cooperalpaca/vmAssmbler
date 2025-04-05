public class Or : Instruction.IInstruction {
    public Or() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b0110 << 24);
    }
}

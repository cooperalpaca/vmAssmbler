public class Not : Instruction.IInstruction {
    public Not() { }

    public int Encode()
    {
        return (0b0011 << 28) | (0b0001 << 24);
    }
}

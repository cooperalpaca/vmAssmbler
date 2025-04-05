public class Lsl : Instruction.IInstruction {
    public Lsl() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b1000 << 24);
    }
}

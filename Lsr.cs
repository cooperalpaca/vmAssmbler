public class Lsr : Instruction.IInstruction {
    public Lsr() {}

    public int Encode() {
        return (0b0010 << 28) | (0b1001 << 24);
    }
}
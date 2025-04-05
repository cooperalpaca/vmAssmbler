public class Xor : Instruction.IInstruction {
    public Xor() {}

    public int Encode() {
        return (0b0010 << 28) | (0b0111 << 24);
    }
}
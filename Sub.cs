public class Sub : Instruction.IInstruction {
    public Sub() {}

    public int Encode() {
        return (0b0010 << 28) | (0b0001 << 24);
    }
}

public class Dump : Instruction.IInstruction {
    public Dump() {}

    public int Encode() {
        return (0b1110 << 28);
    }
}

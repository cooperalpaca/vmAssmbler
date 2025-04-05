public class Input : Instruction.IInstruction {
    public Input() {}

    public int Encode() {
        return 0b0100 << 24;
    }
}

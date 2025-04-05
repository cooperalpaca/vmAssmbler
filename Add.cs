public class Add : Instruction.IInstruction {
    public Add() {}

    public int Encode() {
        return (0b0010 << 28);
    }
}

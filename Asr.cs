public class Asr : Instruction.IInstruction {
    public Asr() { }

    public int Encode()
    {
        return (0b0010 << 28) | (0b1011 << 24);
    }
}

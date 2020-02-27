namespace ForgottenRPG.VM {
    public interface IMemoryPage {
        int ReadAddress(uint offset);
        void WriteAddress(uint offset, int value);
    }
}

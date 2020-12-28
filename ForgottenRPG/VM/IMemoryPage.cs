namespace ForgottenRPG.VM {
    public interface IMemoryPage {
        uint ReadAddress(uint offset);
        void WriteAddress(uint offset, uint value);
    }
}

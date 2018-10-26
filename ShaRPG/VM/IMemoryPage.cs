namespace ShaRPG.VM {
    public interface IMemoryPage {
        int ReadAddress(int offset);
        void WriteAddress(int offset, int value);
    }
}

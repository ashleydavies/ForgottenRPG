namespace ShaRPG.VM {
    public interface IMemoryPage {
        int readAddress(int offset);
        void writeAddress(int offset, int value);
    }
}

namespace ShaRPG.VM {
    public class MemoryPage : IMemoryPage {
        public static const int PageSize = 1024 * 4;
        
        private int[] _memory = new int[PageSize];
        
        public int readAddress(int offset) {
            return _memory[offset];
        }

        public void writeAddress(int offset, int value) {
            _memory[offset] = value;
        }
    }
}

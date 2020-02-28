namespace ForgottenRPG.VM {
    public class MemoryPage : IMemoryPage {
        // 16 kB pages - 4096 32-bit integers per page
        public const uint PageSize = 1024 * 4;
        
        private readonly int[] _memory = new int[PageSize];
        private bool _locked = false;
        
        public int ReadAddress(uint offset) {
            return _memory[offset];
        }

        public void WriteAddress(uint offset, int value) {
            if (_locked) throw new IllegalMemoryAccessException("Attempt to write to protected page");
            _memory[offset] = value;
        }

        public void Lock() {
            _locked = true;
        }
    }
}

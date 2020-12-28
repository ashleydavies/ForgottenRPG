namespace ForgottenRPG.VM {
    public class MemoryPage : IMemoryPage {
        // 16 kB pages - 4096 32-bit integers per page
        public const uint PageSize = 1024 * 4;
        
        private readonly uint[] _memory = new uint[PageSize];
        private bool _locked = false;
        
        public uint ReadAddress(uint offset) {
            return _memory[offset];
        }

        public void WriteAddress(uint offset, uint value) {
            if (_locked) throw new IllegalMemoryAccessException("Attempt to write to protected page");
            _memory[offset] = value;
        }

        public void Lock() {
            _locked = true;
        }
    }
}

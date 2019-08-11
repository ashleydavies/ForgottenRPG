using System;

namespace ShaRPG.VM {
    public class IllegalMemoryAccessException : Exception {
        public IllegalMemoryAccessException(string message) : base(message) { }
    }
}

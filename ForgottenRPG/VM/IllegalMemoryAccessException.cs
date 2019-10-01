using System;

namespace ForgottenRPG.VM {
    public class IllegalMemoryAccessException : Exception {
        public IllegalMemoryAccessException(string message) : base(message) { }
    }
}

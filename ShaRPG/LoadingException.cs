using System;

namespace ShaRPG {
    public class LoadingException : Exception {
        public LoadingException(string message) : base(message) { }
    }
}

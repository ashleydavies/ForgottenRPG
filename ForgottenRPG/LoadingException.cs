using System;

namespace ForgottenRPG {
    public class LoadingException : Exception {
        public LoadingException(string message) : base(message) { }
    }
}

using System;

namespace ForgottenRPG.Entity.Components {
    internal class ComponentException : Exception {
        public ComponentException(string message) : base(message) { }
    }
}

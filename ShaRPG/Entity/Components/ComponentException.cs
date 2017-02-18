using System;

namespace ShaRPG.Entity.Components {
    internal class ComponentException : Exception {
        public ComponentException(string message) : base(message) {}
    }
}

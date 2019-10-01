using System;

namespace ForgottenRPG.Items {
    public class InventoryException : Exception {
        public InventoryException(string message) : base(message) { }
    }
}

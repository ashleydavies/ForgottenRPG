using System;

namespace ForgottenRPG.Inventories {
    public class InventoryException : Exception {
        public InventoryException(string message) : base(message) { }
    }
}

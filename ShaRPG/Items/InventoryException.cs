using System;

namespace ShaRPG.Items {
    public class InventoryException : Exception {
        public InventoryException(string message) : base(message) { }
    }
}

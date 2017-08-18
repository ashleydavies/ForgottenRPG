using System.Collections.Generic;

namespace ShaRPG.Items {
    public class Inventory {
        private const int MaxSize = 36;
        public bool Full => _items.Count == MaxSize;

        private readonly List<ItemStack> _items = new List<ItemStack>(MaxSize);

        public void PickupItem(IItem item, int count = 1) {
            if (Full) throw new InventoryException("Inventory full, item cannot be added");
            _items.Add(new ItemStack(item, count));
        }
    }
}

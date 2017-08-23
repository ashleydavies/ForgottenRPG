using System.Collections.Generic;
using System.Linq;

namespace ShaRPG.Items {
    public class Inventory {
        private const int MaxSize = 30;
        public bool Full => _items.Count == MaxSize;

        private readonly List<ItemStack> _items = new List<ItemStack>(MaxSize);

        public void PickupItem(ItemStack stack) {
            if (Full) throw new InventoryException("Inventory full, item cannot be added");

            ItemStack existing = _items.FindAll(s => s.Item == stack.Item).FirstOrDefault();

            if (existing != null) {
                existing.Merge(stack);
            } else {
                _items.Add(stack);
            }
        }
    }
}

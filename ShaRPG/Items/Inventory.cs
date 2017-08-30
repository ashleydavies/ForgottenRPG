using System.Collections.Generic;
using System.Linq;

namespace ShaRPG.Items {
    public class Inventory {
        private const int MaxSize = 30;
        public bool Full => _items.All(x => x != null);
        
        private readonly ItemStack[] _items = new ItemStack[MaxSize];

        public ItemStack ItemStack(int position) => _items[position];

        public void PickupItem(ItemStack stack) {
            if (Full) throw new InventoryException("Inventory full, item cannot be added");

            ItemStack existing = _items.ToList().FindAll(s => s?.Item == stack.Item).FirstOrDefault();

            if (existing != null) {
                existing.Merge(stack);
            } else {
                _items[_items.ToList().IndexOf(null)] = stack;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ShaRPG.Items {
    public class Inventory {
        public const int MaxSize = 30;
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

        public void InsertToSlot(int slot, ItemStack insert) {
            if (slot > MaxSize || _items[slot] != null && _items[slot].Item != insert.Item) {
                throw new InventoryException("Unable to insert item into designated inventory slot");
            }

            if (_items[slot] == null) {
                _items[slot] = insert;
            } else {
                _items[slot].Merge(insert);
            }
        }

        public ItemStack RemoveFromSlot(int pos) {
            ItemStack itemStack = _items[pos];
            if (itemStack == null) throw new InventoryException("Unable to remove item from slot - none in position");

            _items[pos] = null;
            return itemStack;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using ForgottenRPG.Map;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Items {
    public enum EquipmentSlot {
        Primary = 0,
        Count
    }

    public class Inventory {
        public const int MaxSize = 30;
        public bool Full => _items.All(x => x != null);

        private readonly ItemStack[] _items = new ItemStack[MaxSize];
        private readonly ItemStack[] _equipped = new ItemStack[(int) EquipmentSlot.Count];

        public ItemStack ItemStack(int position) => _items[position];
        public ItemStack EquippedItem(EquipmentSlot slot) => _equipped[(int) slot];

        /// <summary>
        /// Inserts the given ItemStack into the given EquipmentSlot if the slot is empty.
        /// If the slot contains an item, then:
        /// 1. If they are the same item, the stacks are merged. If there is too many items in the given stack to fit
        ///    in the slot, then the original stack minus whatever can fit is returned.
        /// 2. If they are different items, the old item from the slot is returned and the new item is inserted instead.
        /// </summary>
        public ItemStack EquipItem(ItemStack stack, EquipmentSlot slot) {
            int slotId = (int) slot;

            // If the stacks are the same item, attempt to merge them.
            if (EquippedItem(slot) == stack) {
                _equipped[slotId].Merge(stack);
                return stack;
            }

            // Otherwise, swap the equipped item and return what was previously in the slot - possibly null.
            var old = _equipped[slotId];
            _equipped[slotId] = stack;
            return old;
        }

        public void PickupItem(ItemStack stack) {
            var existing = new Queue<ItemStack>(_items.ToList().FindAll(s => s?.Item == stack.Item));

            while (stack.Count > 0 && existing.Count > 0) {
                existing.Dequeue().Merge(stack);
            }

            if (stack.Count > 0) {
                if (Full) throw new InventoryException("Inventory full, item could not be added");
                
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

        public void DropAllItems(IPositionalItemStorage area, GameCoordinate position) {
            foreach (var itemStack in _items) {
                if (itemStack != null) {
                    area.DropItem(position, itemStack);
                }
            }
        }
    }
}

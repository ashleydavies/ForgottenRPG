﻿using System.Collections.Generic;
using System.Linq;
using ShaRPG.Map;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Items {
    public class Inventory {
        public const int MaxSize = 30;
        public bool Full => _items.All(x => x != null);
        
        private readonly ItemStack[] _items = new ItemStack[MaxSize];

        public ItemStack ItemStack(int position) => _items[position];

        public void PickupItem(ItemStack stack) {
            if (Full) throw new InventoryException("Inventory full, item cannot be added");

            var existing = new Queue<ItemStack>(_items.ToList().FindAll(s => s?.Item == stack.Item));

            while (stack.Count > 0 && existing.Count > 0) {
                existing.Dequeue().Merge(stack);
            }

            if (stack.Count > 0) {
                _items[_items.ToList().IndexOf(null)] = stack;
            }
        }

        // TODO: This / the users of this method does / do not follow maximum stack size rules properly
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

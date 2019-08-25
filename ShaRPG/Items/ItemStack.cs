namespace ShaRPG.Items {
    public class ItemStack {
        public readonly IItem Item;
        public int Count { get; private set; }
        
        public ItemStack(IItem item, int count) {
            Item = item;
            Count = count;
        }

        /// <summary>
        /// Attempts to merge two stacks of items. If the stack size is exceeded, `other` will be left with Count > 0
        /// </summary>
        public void Merge(ItemStack other) {
            if (Count + other.Count <= Item.MaxStackSize) {
                Count += other.Count;
                return;
            }

            other.Count -= (Item.MaxStackSize - Count);
            Count = Item.MaxStackSize;
        }
    }
}

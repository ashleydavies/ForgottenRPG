namespace ForgottenRPG.Items {
    public class ItemStack {
        public readonly IItem Item;
        public int Count { get; private set; }
        
        public ItemStack(IItem item, int count) {
            Item = item;
            Count = count;
        }

        public void Merge(ItemStack other) {
            Count += other.Count;
            other.Count = 0;
        }
    }
}

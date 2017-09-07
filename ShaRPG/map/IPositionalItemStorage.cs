using System.Collections.Generic;
using ShaRPG.Items;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map {
    public interface IPositionalItemStorage {
        List<ItemStack> GetItems(GameCoordinate position, int distance);
        void DropItem(GameCoordinate position, ItemStack item);
        void CollectItem(ItemStack itemStack);
    }
}

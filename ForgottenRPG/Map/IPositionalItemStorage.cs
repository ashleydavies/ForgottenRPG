using System.Collections.Generic;
using ForgottenRPG.Items;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Map {
    public interface IPositionalItemStorage {
        List<ItemStack> GetItems(GameCoordinate position, int distance);
        void DropItem(GameCoordinate position, ItemStack item);
        void CollectItem(ItemStack itemStack);
    }
}

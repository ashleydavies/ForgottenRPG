using System.Collections.Generic;
using ShaRPG.Items;

namespace ShaRPG.GameState {
    public partial class InventoryState {
        private const int WindowSizeX = 60 * 9;
        private const int WindowSizeY = 60 * 10;
        private const int TileSize = 48;
        private const int TilesX = 10;
        private const int TilesY = 3;
        private const int TilesMargin = 2;
        private const int NearbyItemsX = 2;
        private const int NearbyItemsY = 7;
        private const int TilesEdgeMargin = (WindowSizeX - TilesX * (TileSize + TilesMargin)) / 2;
        private const int NearbyItemSearchDistance = 80;

        private List<ItemStack> NearbyItems =>
            _positionalItemStorage.GetItems(_playerPosition, NearbyItemSearchDistance);
    }
}

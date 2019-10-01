using System.Collections.Generic;
using System.Linq.Expressions;
using ForgottenRPG.Items;
using SFML.System;

namespace ForgottenRPG.GameState {
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

        private static readonly Vector2i Size = new Vector2i(WindowSizeX, WindowSizeY);
        private static readonly Vector2i PlayerSpritePosition = new Vector2i(120, 20);

        private static readonly Dictionary<EquipmentSlot, Vector2i> EquipmentPositions =
            new Dictionary<EquipmentSlot, Vector2i> {
                { EquipmentSlot.Primary, new Vector2i(90, 220) },
                { EquipmentSlot.Headgear, new Vector2i(120, 30) }
            };

        private List<ItemStack> NearbyItems =>
            _positionalItemStorage.GetItems(_playerPosition, NearbyItemSearchDistance);
    }
}

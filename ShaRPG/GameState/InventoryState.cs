using ShaRPG.Camera;
using ShaRPG.GUI;
using ShaRPG.Items;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    public class InventoryState : AbstractGameState {
        private readonly GuiWindow _guiWindow;
        private readonly Inventory _inventory;
        private static readonly Vector2I Size = new Vector2I(WindowSizeX + 24, WindowSizeY + 24);
        private const int WindowSizeX = 60 * 9;
        private const int WindowSizeY = 60 * 12;
        private const int TileSize = 48;
        private const int TilesX = 10;
        private const int TilesY = 3;
        private const int TilesMargin = 2;
        private const int TilesEdgeMargin = (WindowSizeX - TilesX * (TileSize + TilesMargin)) / 2;

        public InventoryState(Game game, Inventory inventory, Vector2I windowSize, ICamera camera,
                              ISpriteStoreService spriteStore)
            : base(game, camera) {
            _inventory = inventory;

            _guiWindow = new GuiWindow(spriteStore, windowSize / 2, Size);

            Sprite itemSlotSprite = spriteStore.GetSprite("ui_item_slot");

            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 10; x++) {
                    int xPosition = TilesEdgeMargin + x * (TileSize + TilesMargin);
                    int yPosition = WindowSizeY - TilesEdgeMargin - (TilesY - y) * (TileSize + TilesMargin);

                    _guiWindow.AddComponent(
                        new FixedContainer(xPosition, yPosition, new SpriteContainer(itemSlotSprite)));
                }
            }
        }

        public override void Update(float delta) { }

        public override void Render(IRenderSurface renderSurface) {
            _guiWindow.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_guiWindow.IsMouseOver(coordinates)) _guiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) { }
    }
}

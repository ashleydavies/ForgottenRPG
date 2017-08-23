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
        private static readonly Vector2I GuiWindowSize = new Vector2I(60 * 9 + 24, 60 * 12 + 24);
        
        public InventoryState(Game game, Inventory inventory, Vector2I windowSize, ICamera camera, ISpriteStoreService spriteStore)
            : base(game, camera) {
            _inventory = inventory;
            
            _guiWindow = new GuiWindow(spriteStore, windowSize / 2, GuiWindowSize);
        }
        
        public override void Update(float delta) {
        }

        public override void Render(IRenderSurface renderSurface) {
            _guiWindow.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_guiWindow.IsMouseOver(coordinates)) _guiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) {
            
        }
    }
}

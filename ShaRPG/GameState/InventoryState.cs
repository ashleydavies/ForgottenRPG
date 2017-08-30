using SFML.Window;
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

        private SpriteContainer[] _inventoryItemSpriteContainers = new SpriteContainer[Inventory.MaxSize];
        private ItemStack _heldItemStack;

        public InventoryState(Game game, Inventory inventory, Vector2I windowSize, ICamera camera,
                              ISpriteStoreService spriteStore)
            : base(game, camera) {
            _inventory = inventory;

            _guiWindow = new GuiWindow(spriteStore, windowSize / 2, Size);

            Sprite itemSlotSprite = spriteStore.GetSprite("ui_item_slot");

            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 10; x++) {
                    int pos = y * TilesX + x;
                    int xPosition = TilesEdgeMargin + x * (TileSize + TilesMargin);
                    int yPosition = WindowSizeY - TilesEdgeMargin - (TilesY - y) * (TileSize + TilesMargin);

                    SpriteContainer slotContainer = new SpriteContainer(itemSlotSprite);
                    slotContainer.OnClicked += _ => SlotClicked(pos);

                    _guiWindow.AddComponent(new FixedContainer(xPosition, yPosition, slotContainer));

                    SpriteContainer spriteContainer = new SpriteContainer(inventory.ItemStack(pos) != null
                                                                              ? inventory.ItemStack(pos).Item.Sprite
                                                                              : Sprite.Null);
                    _inventoryItemSpriteContainers[pos] = spriteContainer;
                    _guiWindow.AddComponent(new FixedContainer(
                                                xPosition + TileSize / 2 - ItemManager.SpriteSizeX / 2,
                                                yPosition + TileSize / 2 - ItemManager.SpriteSizeY / 2,
                                                spriteContainer));
                }
            }
        }

        private void SlotClicked(int pos) {
            ServiceLocator.LogService.Log(LogType.Information, $"Clicked slot {pos}");

            ItemStack inSlot = _inventory.ItemStack(pos);

            if (inSlot == null && _heldItemStack == null) {
                return;
            }

            if (inSlot == null) {
                _inventory.InsertToSlot(pos, _heldItemStack);
                _inventoryItemSpriteContainers[pos].Sprite = _heldItemStack.Item.Sprite;
                _heldItemStack = null;
            } else {
                _heldItemStack = _inventory.RemoveFromSlot(pos);
                _inventoryItemSpriteContainers[pos].Sprite = Sprite.Null;
            }
        }

        public override void Update(float delta) {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape)) {
                EndState();
            }
        }

        public override void Render(IRenderSurface renderSurface) {
            _guiWindow.Render(renderSurface);
            if (_heldItemStack != null) {
                renderSurface.Render(_heldItemStack.Item.Sprite,
                                     new ScreenCoordinate(SFML.Window.Mouse.GetPosition().X,
                                                          SFML.Window.Mouse.GetPosition().Y));
            }
        }
        public override void Clicked(ScreenCoordinate coordinates) {
            if (_guiWindow.IsMouseOver(coordinates)) _guiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) { }
    }
}

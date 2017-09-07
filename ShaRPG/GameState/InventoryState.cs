using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.GUI;
using ShaRPG.Items;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    public partial class InventoryState : AbstractGameState {
        private readonly GuiWindow _guiWindow;
        private readonly Inventory _inventory;
        private static readonly Vector2i Size = new Vector2i(WindowSizeX, WindowSizeY);
        private readonly SpriteContainer[] _inventoryItemContainers = new SpriteContainer[Inventory.MaxSize];
        private readonly SpriteContainer[] _nearbyItemContainers = new SpriteContainer[NearbyItemsX * NearbyItemsY];
        private ItemStack _heldItemStack;

        public InventoryState(Game game, Inventory inventory, Vector2f windowSize, ITextureStore textureStore)
            : base(game) {
            _inventory = inventory;

            _guiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), Size);

            Sprite itemSlotSprite = textureStore.GetNewSprite("ui_item_slot");

            for (int y = 0; y < 3; y++) {
                for (int x = 0; x < 10; x++) {
                    int pos = y * TilesX + x;
                    int xPosition = TilesEdgeMargin + x * (TileSize + TilesMargin);
                    int yPosition = WindowSizeY - TilesEdgeMargin - (TilesY - y) * (TileSize + TilesMargin);

                    SpriteContainer slotContainer = new SpriteContainer(itemSlotSprite);
                    slotContainer.OnClicked += _ => SlotClicked(pos);

                    _guiWindow.AddComponent(new FixedContainer(xPosition, yPosition, slotContainer));

                    SpriteContainer spriteContainer = new SpriteContainer(inventory.ItemStack(pos) != null
                                                                              ? inventory.ItemStack(pos).Item.Texture
                                                                              : new Sprite());
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
                _inventoryItemSpriteContainers[pos].Sprite = _heldItemStack.Item.Texture;
                _heldItemStack = null;
            } else {
                _heldItemStack = _inventory.RemoveFromSlot(pos);
                _inventoryItemSpriteContainers[pos].Sprite = new Sprite();
            }
        }

        public override void Update(float delta) {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape)) {
                EndState();
            }
        }

        public override void Render(RenderTarget renderSurface) {
            _guiWindow.Render(renderSurface);
            if (_heldItemStack != null) {
                renderSurface.Draw(new Sprite(_heldItemStack.Item.Texture) {
                    Position = new ScreenCoordinate(Mouse.GetPosition())
                });
            }
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_guiWindow.IsMouseOver(coordinates)) _guiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) { }
    }
}

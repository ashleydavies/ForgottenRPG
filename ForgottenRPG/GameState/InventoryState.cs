using System.Collections.Generic;
using ForgottenRPG.GUI;
using ForgottenRPG.Items;
using ForgottenRPG.Map;
using ForgottenRPG.Service;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ForgottenRPG.GameState {
    public partial class InventoryState : AbstractGameState {
        private readonly GuiWindow _guiWindow;
        private readonly GuiWindow _tooltipWindow;
        private readonly TextContainer _tooltipTitle;
        private readonly SpriteContainer _tooltipSprite;
        private readonly TextContainer _tooltipText;
        private readonly Inventory _inventory;
        private static readonly Vector2i Size = new Vector2i(WindowSizeX, WindowSizeY);
        private readonly SpriteContainer[] _inventoryItemContainers = new SpriteContainer[Inventory.MaxSize];
        private readonly SpriteContainer[] _nearbyItemContainers = new SpriteContainer[NearbyItemsX * NearbyItemsY];
        private ItemStack _heldItemStack;
        private readonly IPositionalItemStorage _positionalItemStorage;
        private readonly GameCoordinate _playerPosition;

        public InventoryState(Game game, Inventory inventory, IPositionalItemStorage closeItemStorage,
                              GameCoordinate playerPos, Vector2f windowSize, ITextureStore textureStore) : base(game) {
            _positionalItemStorage = closeItemStorage;
            _playerPosition = playerPos;
            _inventory = inventory;

            _guiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), Size);

            Sprite itemSlotSprite = textureStore.GetNewSprite("ui_item_slot");

            for (int y = 0; y < TilesY; y++) {
                for (int x = 0; x < TilesX; x++) {
                    int pos = y * TilesX + x;
                    int xPosition = TilesEdgeMargin + x * (TileSize + TilesMargin);
                    int yPosition = WindowSizeY - TilesEdgeMargin - (TilesY - y) * (TileSize + TilesMargin);

                    SpriteContainer slotContainer = new SpriteContainer(itemSlotSprite);
                    slotContainer.OnClicked += _ => SlotClicked(pos);

                    _guiWindow.AddComponent(new FixedContainer(xPosition, yPosition, slotContainer));

                    _inventoryItemContainers[pos] = new SpriteContainer(inventory.ItemStack(pos) != null
                                                                            ? inventory.ItemStack(pos).Item.Texture
                                                                            : new Sprite());
                    _guiWindow.AddComponent(new FixedContainer(xPosition + TileSize / 2 - ItemManager.SpriteSizeX / 2,
                                                               yPosition + TileSize / 2 - ItemManager.SpriteSizeY / 2,
                                                               _inventoryItemContainers[pos]));
                }
            }

            for (int y = 0; y < NearbyItemsY; y++) {
                for (int x = 0; x < NearbyItemsX; x++) {
                    int pos = y * NearbyItemsX + x;
                    int xPosition = WindowSizeX - TilesEdgeMargin - (2 - x) * (TileSize + TilesMargin);
                    int yPosition = TilesEdgeMargin + TilesMargin + y * (TileSize + TilesMargin);

                    SpriteContainer slotContainer = new SpriteContainer(itemSlotSprite);
                    slotContainer.OnClicked += _ => NearbySlotClicked(pos);

                    _guiWindow.AddComponent(new FixedContainer(xPosition, yPosition, slotContainer));

                    _nearbyItemContainers[pos] = new SpriteContainer(new Sprite());
                    _guiWindow.AddComponent(new FixedContainer(xPosition + TileSize / 2 - ItemManager.SpriteSizeX / 2,
                                                               yPosition + TileSize / 2 - ItemManager.SpriteSizeY / 2,
                                                               _nearbyItemContainers[pos]));
                }
            }

            UpdateNearbyItemSprites();
            
            _tooltipWindow = new GuiWindow(textureStore, new Vector2i(0, 0), new Vector2i(60 * 5, 60 * 3));
            {
                _tooltipTitle = new TextContainer("Title", 32);
                _tooltipSprite = new SpriteContainer(new Sprite());
                _tooltipText = new TextContainer("Hello, world", 18);
                
                var tooltipSplitBottom = new ColumnContainer(ColumnContainer.Side.Left, 80);
                tooltipSplitBottom.SetLeftComponent(_tooltipSprite);
                tooltipSplitBottom.SetRightComponent(_tooltipText);
                
                var tooltipSplit = new VerticalFlowContainer();
                tooltipSplit.AddComponent(_tooltipTitle);
                tooltipSplit.AddComponent(new PaddingContainer(8, null));
                tooltipSplit.AddComponent(tooltipSplitBottom);
                
                _tooltipWindow.AddComponent(new PaddingContainer(8, tooltipSplit));
            }
        }

        private void SlotClicked(int pos) {
            ServiceLocator.LogService.Log(LogType.Info, $"Clicked inventory slot {pos}");

            ItemStack inSlot = _inventory.ItemStack(pos);

            // If there is nothing in the slot and they are not holding an item, we have nothing to do
            if (inSlot == null && _heldItemStack == null) {
                return;
            }

            // Otherwise, handle the cases of dropping into an empty slot, dropping into an occupied slot, or collecting
            //  from an occupied slot.
            if (_heldItemStack != null && inSlot == null) {
                _inventory.InsertToSlot(pos, _heldItemStack);
                _heldItemStack = null;
                Game.ShowMouse();
            } else if (_heldItemStack != null) {
                var previouslyHeldItem = _heldItemStack;
                _heldItemStack = _inventory.RemoveFromSlot(pos);
                _inventory.InsertToSlot(pos, previouslyHeldItem);
            } else if (inSlot != null) {
                _heldItemStack = _inventory.RemoveFromSlot(pos);
                Game.HideMouse();
            }

            _inventoryItemContainers[pos].Sprite = _inventory.ItemStack(pos)?.Item?.Texture ?? new Sprite();
        }

        private void NearbySlotClicked(int pos) {
            ServiceLocator.LogService.Log(LogType.Info, $"Clicked nearby slot {pos}");

            if (_heldItemStack != null) {
                _positionalItemStorage.DropItem(_playerPosition, _heldItemStack);
                _heldItemStack = null;
                UpdateNearbyItemSprites();
                Game.ShowMouse();
            } else {
                ItemStack inSlot = pos < NearbyItems.Count ? NearbyItems[pos] : null;

                if (inSlot != null) {
                    _heldItemStack = inSlot;
                    _positionalItemStorage.CollectItem(inSlot);
                    UpdateNearbyItemSprites();
                    Game.HideMouse();
                }
            }
        }

        private void UpdateNearbyItemSprites() {
            List<ItemStack> nearbyItems = NearbyItems;
            for (int i = 0; i < _nearbyItemContainers.Length; i++) {
                if (nearbyItems.Count > i) {
                    _nearbyItemContainers[i].Sprite = new Sprite(nearbyItems[i].Item.Texture);
                } else {
                    _nearbyItemContainers[i].Sprite = new Sprite();
                }
            }
        }

        public override void Update(float delta) {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape)) {
                EndState();
            }
        }

        public override void Render(RenderTarget renderSurface) {
            _guiWindow.Render(renderSurface);

            for (var i = 0; i < _inventoryItemContainers.Length; i++) {
                TryRenderTooltip(renderSurface, _inventoryItemContainers[i], _inventory.ItemStack(i));
            }

            for (var i = 0; i < _nearbyItemContainers.Length; i++) {
                if (NearbyItems.Count <= i) break;
                TryRenderTooltip(renderSurface, _nearbyItemContainers[i], NearbyItems[i]);
            }

            if (_heldItemStack != null) {
                renderSurface.Draw(new Sprite(_heldItemStack.Item.Texture) {
                    Position = Game.MousePosition - new ScreenCoordinate(ItemManager.SpriteSize) / 2
                });
            }
        }

        private void TryRenderTooltip(RenderTarget renderSurface, SpriteContainer slot, ItemStack itemStack) {
            if (!slot.IsMouseOver(Game.MousePosition)) return;
            _tooltipTitle.Contents = itemStack.Item.DisplayName;
            _tooltipText.Contents = itemStack.Item.Description;
            // TODO: Can this be converted to a render matrix in SpriteContainer or something?
            _tooltipSprite.Sprite = new Sprite(itemStack.Item.Texture) { Scale = new Vector2f(2, 2) };

            _tooltipWindow.ScreenPosition = Game.MousePosition + new ScreenCoordinate(32, 32);
            _tooltipWindow.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_guiWindow.IsMouseOver(coordinates)) _guiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(float delta) { }
    }
}

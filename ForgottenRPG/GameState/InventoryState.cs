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
        private readonly Inventory _inventory;
        private readonly SpriteContainer[] _inventoryItemContainers = new SpriteContainer[Inventory.MaxSize];
        private readonly SpriteContainer[] _nearbyItemContainers = new SpriteContainer[NearbyItemsX * NearbyItemsY];
        private readonly SpriteContainer[] _equipmentContainers = new SpriteContainer[EquipmentPositions.Count];
        private readonly IPositionalItemStorage _positionalItemStorage;
        private readonly GameCoordinate _playerPosition;
        private GuiWindow _tooltipWindow;
        private TextContainer _tooltipTitle;
        private SpriteContainer _tooltipSprite;
        private TextContainer _tooltipText;
        private ItemStack _heldItemStack;

        private ItemStack HeldItemStack {
            get => _heldItemStack;
            set {
                if (value == null) Game.ShowMouse();
                else Game.HideMouse();
                _heldItemStack = value;
            }
        }

        public InventoryState(Game game, Inventory inventory, IPositionalItemStorage closeItemStorage,
                              GameCoordinate playerPos, Vector2f windowSize, ITextureStore textureStore) : base(game) {
            if (_equipmentContainers.Length != (int) EquipmentSlot.Count) {
                throw new InventoryException("InventoryState's understanding of equipment is out of date");
            }

            if (TilesX * TilesY != Inventory.MaxSize) {
                throw new InventoryException("InventoryState's understanding of inventory size is out of date");
            }

            _positionalItemStorage = closeItemStorage;
            _playerPosition = playerPos;
            _inventory = inventory;

            _guiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), Size);
            var playerSprite = textureStore.GetNewSprite("ui_avatar_player");
            playerSprite.Scale = new Vector2f(6, 6);
            _guiWindow.AddComponent(new FixedContainer(PlayerSpritePosition, new SpriteContainer(playerSprite)));

            Sprite itemSlotSprite = textureStore.GetNewSprite("ui_item_slot");

            InitialiseInventorySlots(inventory, itemSlotSprite);
            InitialiseNearbyItemSlots(itemSlotSprite);
            InitialiseTooltip(textureStore);
            InitialiseEquipmentSlots(itemSlotSprite);
        }

        private void SlotClicked(int pos) {
            ServiceLocator.LogService.Log(LogType.Info, $"Clicked inventory slot {pos}");

            ItemStack inSlot = _inventory.ItemStack(pos);

            // If there is nothing in the slot and they are not holding an item, we have nothing to do
            if (inSlot == null && HeldItemStack == null) {
                return;
            }

            // Otherwise, handle the cases of dropping into an empty slot, dropping into an occupied slot, or collecting
            //  from an occupied slot.
            if (HeldItemStack != null && inSlot == null) {
                _inventory.InsertToSlot(pos, HeldItemStack);
                HeldItemStack = null;
            } else if (HeldItemStack != null) {
                var previouslyHeldItem = HeldItemStack;
                HeldItemStack = _inventory.RemoveFromSlot(pos);
                _inventory.InsertToSlot(pos, previouslyHeldItem);
            } else if (inSlot != null) {
                HeldItemStack = _inventory.RemoveFromSlot(pos);
            }

            _inventoryItemContainers[pos].Sprite = _inventory.ItemStack(pos)?.Item?.Texture ?? new Sprite();
        }

        private void EquipmentSlotClicked(EquipmentSlot slot) {
            HeldItemStack = _inventory.EquipItem(HeldItemStack, slot);
            _equipmentContainers[(int) slot].Sprite = _inventory.EquippedItem(slot)?.Item?.Texture ?? new Sprite();
        }

        private void NearbySlotClicked(int pos) {
            ServiceLocator.LogService.Log(LogType.Info, $"Clicked nearby slot {pos}");

            if (HeldItemStack != null) {
                _positionalItemStorage.DropItem(_playerPosition, HeldItemStack);
                HeldItemStack = null;
                UpdateNearbyItemSprites();
            } else {
                ItemStack inSlot = pos < NearbyItems.Count ? NearbyItems[pos] : null;

                if (inSlot != null) {
                    HeldItemStack = inSlot;
                    _positionalItemStorage.CollectItem(inSlot);
                    UpdateNearbyItemSprites();
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

            for (var i = 0; i < _equipmentContainers.Length; i++) {
                TryRenderTooltip(renderSurface, _equipmentContainers[i], _inventory.EquippedItem((EquipmentSlot) i));
            }

            if (HeldItemStack != null) {
                renderSurface.Draw(new Sprite(HeldItemStack.Item.Texture) {
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

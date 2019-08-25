using ForgottenRPG.GUI;
using ForgottenRPG.Items;
using ForgottenRPG.Service;
using SFML.Graphics;
using SFML.System;

namespace ForgottenRPG.GameState {
    public partial class InventoryState {
        private void InitialiseNearbyItemSlots(Sprite itemSlotSprite) {
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
        }

        private void InitialiseInventorySlots(Inventory inventory, Sprite itemSlotSprite) {
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
        }

        private void InitialiseTooltip(ITextureStore textureStore) {
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

        private void InitialiseEquipmentSlots(Sprite itemSlotSprite) {
            for (int slotNo = 0; slotNo < (int) EquipmentSlot.Count; slotNo++) {
                EquipmentSlot slot = (EquipmentSlot) slotNo;
                int xPosition = EquipmentPositions[slot].X;
                int yPosition = EquipmentPositions[slot].Y;

                SpriteContainer slotContainer = new SpriteContainer(itemSlotSprite);
                slotContainer.OnClicked += _ => EquipmentSlotClicked(slot);

                _guiWindow.AddComponent(new FixedContainer(xPosition, yPosition, slotContainer));

                _equipmentContainers[slotNo] = new SpriteContainer(_inventory.EquippedItem(slot) != null
                                                                       ? _inventory.EquippedItem(slot).Item.Texture
                                                                       : new Sprite());
                _guiWindow.AddComponent(new FixedContainer(xPosition + TileSize / 2 - ItemManager.SpriteSizeX / 2,
                                                           yPosition + TileSize / 2 - ItemManager.SpriteSizeY / 2,
                                                           _equipmentContainers[slotNo]));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using ForgottenRPG.Entity;
using ForgottenRPG.Entity.Components;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Items;
using ForgottenRPG.NativeFunctions;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Scripting {
    /// <summary>
    /// Native functions library for game-script interaction.
    /// All functions marked with [NativeFunction] can be called from scripts.
    /// </summary>
    public static class GameScriptLibrary {
        [NativeFunction("_internal_commit_data")]
        public static int InternalCommitData(int address, ScriptNativeContext ctx) {
            if (ctx.PendingData.Count == 0) return address;
            
            object data = ctx.PendingData.Pop();
            uint addr = (uint)address;
            
            if (data is string s) {
                ctx.WriteMemory(addr++, (uint)s.Length);
                foreach (char c in s) {
                    ctx.WriteMemory(addr++, c);
                }
            } else if (data is System.Collections.IEnumerable enumerable) {
                // Count elements first to write length
                int count = 0;
                foreach (var _ in enumerable) count++;
                
                ctx.WriteMemory(addr++, (uint)count);
                
                foreach (var item in enumerable) {
                    // For now we assume primitive types or handles that fit in a word
                    // In a more complex system we'd recursively marshal here
                    if (item is int i) ctx.WriteMemory(addr++, (uint)i);
                    else if (item is uint u) ctx.WriteMemory(addr++, u);
                    else if (item is float f) {
                        byte[] bytes = BitConverter.GetBytes(f);
                        ctx.WriteMemory(addr++, BitConverter.ToUInt32(bytes, 0));
                    }
                    else if (item is bool b) ctx.WriteMemory(addr++, b ? 1u : 0u);
                    else {
                        // Handle reference types as handles
                        int id = ctx.RegisterObject(item);
                        ctx.WriteMemory(addr++, (uint)id);
                    }
                }
            }
            
            return address;
        }

        // The player entity is stored in a special slot that will be set during initialization
        private static GameEntity _playerEntity;
        private static ItemManager _itemManager;

        /// <summary>
        /// Initialize the library with the player entity reference
        /// </summary>
        public static void Initialize(GameEntity player, ItemManager itemManager) {
            _playerEntity = player;
            _itemManager = itemManager;
        }

        // ===== Player Access =====

        /// <summary>
        /// Get a handle to the player entity
        /// Returns: entity handle (Entity*)
        /// </summary>
        [NativeFunction("get_player")]
        public static GameEntity GetPlayer() {
            if (_playerEntity == null) {
                throw new InvalidOperationException("Player entity not initialized in GameScriptLibrary");
            }
            return _playerEntity;
        }

        // ===== Health Operations =====

        /// <summary>
        /// Get the current health of an entity
        /// Parameters: entity handle (Entity*)
        /// Returns: current health (float)
        /// </summary>
        [NativeFunction("entity_get_health")]
        public static float EntityGetHealth(GameEntity entity) {
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) {
                throw new InvalidOperationException($"Entity {entity.Name} does not have a HealthComponent");
            }
            
            // Use reflection to access the private Health property
            var healthProperty = typeof(HealthComponent).GetProperty("Health",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (healthProperty == null) {
                throw new InvalidOperationException("Could not access Health property");
            }
            
            return (float)healthProperty.GetValue(healthComponent);
        }

        /// <summary>
        /// Get the maximum health of an entity
        /// Parameters: entity handle (Entity*)
        /// Returns: max health (int)
        /// </summary>
        [NativeFunction("entity_get_max_health")]
        public static int EntityGetMaxHealth(GameEntity entity) {
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) {
                throw new InvalidOperationException($"Entity {entity.Name} does not have a HealthComponent");
            }
            
            // Use reflection to access the private _maxHealth field
            var maxHealthField = typeof(HealthComponent).GetField("_maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (maxHealthField == null) {
                throw new InvalidOperationException("Could not access _maxHealth field");
            }
            
            return (int)maxHealthField.GetValue(healthComponent);
        }

        /// <summary>
        /// Check if an entity is dead
        /// Parameters: entity handle (Entity*)
        /// Returns: true if dead, false otherwise (bool)
        /// </summary>
        [NativeFunction("entity_is_dead")]
        public static bool EntityIsDead(GameEntity entity) {
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) {
                return false; // No health component means not subject to death
            }
            return healthComponent.Dead;
        }

        // ===== Damage Events =====

        /// <summary>
        /// Send damage to an entity
        /// Parameters: entity handle (Entity*), damage amount (int)
        /// </summary>
        [NativeFunction("entity_send_damage")]
        public static void EntitySendDamage(GameEntity entity, int damageAmount) {
            entity.SendMessage(new DamageMessage(damageAmount, _playerEntity));
        }

        // ===== Equipment Inspection =====

        /// <summary>
        /// Check if entity has an item equipped in primary slot
        /// Parameters: entity handle (Entity*)
        /// Returns: true if equipped, false otherwise (bool)
        /// </summary>
        [NativeFunction("entity_has_primary_equipped")]
        public static bool EntityHasPrimaryEquipped(GameEntity entity) {
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null) {
                return false;
            }
            
            var equipped = inventoryComponent.Inventory.EquippedItem(EquipmentSlot.Primary);
            return equipped != null;
        }

        /// <summary>
        /// Get the item equipped in primary slot
        /// Parameters: entity handle (Entity*)
        /// Returns: item handle (Item*) or null
        /// </summary>
        [NativeFunction("entity_get_primary_item")]
        public static IItem EntityGetPrimaryItem(GameEntity entity) {
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null) {
                return null;
            }
            
            var equipped = inventoryComponent.Inventory.EquippedItem(EquipmentSlot.Primary);
            return equipped?.Item;
        }

        /// <summary>
        /// Get item name hash
        /// Parameters: item handle (Item*)
        /// Returns: hash of item name (int)
        /// </summary>
        [NativeFunction("test_get_list")]
        public static List<int> TestGetList() {
            return new List<int> { 10, 20, 30, 40, 50 };
        }

        [NativeFunction("item_get_name")]
        public static string ItemGetName(IItem item) {
            return item?.Name ?? "Unknown Item";
        }

        [NativeFunction("item_get_name_hash")]
        public static int ItemGetNameHash(IItem item) {
            if (item == null) return -1;
            return item.Name.GetHashCode();
        }

        // ===== Component Checks =====

        /// <summary>
        /// Check if entity has a health component
        /// Parameters: entity handle (Entity*)
        /// Returns: true if has health component, false otherwise (bool)
        /// </summary>
        [NativeFunction("entity_has_health")]
        public static bool EntityHasHealth(GameEntity entity) {
            return entity.GetComponent<HealthComponent>() != null;
        }

        /// <summary>
        /// Check if entity has an inventory component
        /// Parameters: entity handle (Entity*)
        /// Returns: true if has inventory component, false otherwise (bool)
        /// </summary>
        [NativeFunction("entity_has_inventory")]
        public static bool EntityHasInventory(GameEntity entity) {
            return entity.GetComponent<InventoryComponent>() != null;
        }

        /// <summary>
        /// Check if entity has a combat component
        /// Parameters: entity handle (Entity*)
        /// Returns: true if has combat component, false otherwise (bool)
        /// </summary>
        [NativeFunction("entity_has_combat")]
        public static bool EntityHasCombat(GameEntity entity) {
            return entity.GetComponent<CombatComponent>() != null;
        }

        // ===== Entity Info =====

        /// <summary>
        /// Get entity name (returns hash for simplicity)
        /// Parameters: entity handle (Entity*)
        /// Returns: hash of entity name (int)
        /// </summary>
        [NativeFunction("entity_get_name_hash")]
        public static int EntityGetNameHash(GameEntity entity) {
            return entity.Name.GetHashCode();
        }

        /// <summary>
        /// Get entity ID
        /// Parameters: entity handle (Entity*)
        /// Returns: entity ID (int)
        /// </summary>
        [NativeFunction("entity_get_id")]
        public static int EntityGetId(GameEntity entity) {
            return entity.Id;
        }

        // ===== Position =====

        /// <summary>
        /// Get the entity world position (cartesian)
        /// Parameters: entity handle (Entity*), output pointer (Vector2*)
        /// </summary>
        [NativeFunction("entity_get_position")]
        public static void EntityGetPosition(GameEntity entity, ScriptValuePtr<Vector2F> outPos, ScriptNativeContext ctx) {
            var pos = (GameCoordinate)entity.Position;
            ctx.WriteValue((uint)outPos.Address, new Vector2F(pos.X, pos.Y));
        }

        /// <summary>
        /// Get the entity tile position
        /// Parameters: entity handle (Entity*), output pointer (Vector2I*)
        /// </summary>
        [NativeFunction("entity_get_tile_position")]
        public static void EntityGetTilePosition(GameEntity entity, ScriptValuePtr<Vector2I> outPos, ScriptNativeContext ctx) {
            var pos = entity.Position;
            ctx.WriteValue((uint)outPos.Address, new Vector2I(pos.X, pos.Y));
        }

        /// <summary>
        /// Check if an entity is alive
        /// Parameters: entity handle (Entity*)
        /// Returns: true if alive (bool)
        /// </summary>
        [NativeFunction("entity_is_alive")]
        public static bool EntityIsAlive(GameEntity entity) {
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) return true;
            return !healthComponent.Dead;
        }

        /// <summary>
        /// Heal an entity
        /// Parameters: entity handle (Entity*), heal amount (int)
        /// </summary>
        [NativeFunction("entity_heal")]
        public static void EntityHeal(GameEntity entity, int amount) {
            if (amount <= 0) return;
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) return;

            var healthProperty = typeof(HealthComponent).GetProperty("Health",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (healthProperty == null) {
                throw new InvalidOperationException("Could not access Health property");
            }

            float current = (float)healthProperty.GetValue(healthComponent);
            healthProperty.SetValue(healthComponent, current + amount);
        }

        /// <summary>
        /// Set an entity's health value directly
        /// Parameters: entity handle (Entity*), health value (float)
        /// </summary>
        [NativeFunction("entity_set_health")]
        public static void EntitySetHealth(GameEntity entity, float value) {
            var healthComponent = entity.GetComponent<HealthComponent>();
            if (healthComponent == null) return;

            var healthProperty = typeof(HealthComponent).GetProperty("Health",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (healthProperty == null) {
                throw new InvalidOperationException("Could not access Health property");
            }

            healthProperty.SetValue(healthComponent, value);
        }

        /// <summary>
        /// Get an item by ID
        /// Parameters: item id (int)
        /// Returns: item handle (Item*)
        /// </summary>
        [NativeFunction("item_get_by_id")]
        public static IItem ItemGetById(int id) {
            if (_itemManager == null) return null;
            return _itemManager.GetItem(id);
        }

        /// <summary>
        /// Get an item by codename
        /// Parameters: codename (string)
        /// Returns: item handle (Item*)
        /// </summary>
        [NativeFunction("item_get_by_codename")]
        public static IItem ItemGetByCodename(string codename) {
            if (_itemManager == null) return null;
            var item = _itemManager.GetItem(codename);
            return _itemManager.GetItem(codename);
        }

        [NativeFunction("item_get_id")]
        public static int ItemGetId(IItem item) {
            return item?.Id ?? -1;
        }

        [NativeFunction("item_get_codename")]
        public static string ItemGetCodename(IItem item) {
            return item?.Codename ?? "";
        }

        [NativeFunction("item_get_display_name")]
        public static string ItemGetDisplayName(IItem item) {
            return item?.DisplayName ?? "";
        }

        [NativeFunction("item_get_description")]
        public static string ItemGetDescription(IItem item) {
            return item?.Description ?? "";
        }

        [NativeFunction("item_get_max_stack")]
        public static int ItemGetMaxStack(IItem item) {
            return item?.MaxStackSize ?? 0;
        }

        [NativeFunction("entity_get_item_count")]
        public static int EntityGetItemCount(GameEntity entity, IItem item) {
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null || item == null) return 0;
            return inventoryComponent.Inventory.CountItem(item);
        }

        [NativeFunction("entity_has_item")]
        public static bool EntityHasItem(GameEntity entity, IItem item, int count) {
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null || item == null) return false;
            return inventoryComponent.Inventory.HasItem(item, count);
        }

        [NativeFunction("entity_add_item")]
        public static bool EntityAddItem(GameEntity entity, IItem item, int count) {
            if (item == null || count <= 0) return false;
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null) return false;

            try {
                inventoryComponent.Inventory.PickupItem(new ItemStack(item, count));
                return true;
            } catch (InventoryException) {
                return false;
            }
        }

        [NativeFunction("entity_remove_item")]
        public static int EntityRemoveItem(GameEntity entity, IItem item, int count) {
            if (item == null || count <= 0) return 0;
            var inventoryComponent = entity.GetComponent<InventoryComponent>();
            if (inventoryComponent == null) return 0;
            return inventoryComponent.Inventory.RemoveItem(item, count);
        }
    }
}

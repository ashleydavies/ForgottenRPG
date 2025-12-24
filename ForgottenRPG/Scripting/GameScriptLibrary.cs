using System;
using System.Collections.Generic;
using ForgottenRPG.Entity;
using ForgottenRPG.Entity.Components;
using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Items;
using ForgottenRPG.NativeFunctions;

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

        /// <summary>
        /// Initialize the library with the player entity reference
        /// </summary>
        public static void Initialize(GameEntity player) {
            _playerEntity = player;
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
    }
}

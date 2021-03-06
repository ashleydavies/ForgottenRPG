﻿using ForgottenRPG.Entity.Components.Messages;
using ForgottenRPG.Items;
using ForgottenRPG.Map;

namespace ForgottenRPG.Entity.Components {
    public class InventoryComponent : AbstractComponent, IMessageHandler<DiedMessage> {
        public readonly Inventory Inventory = new Inventory();
        
        private readonly IPositionalItemStorage _itemDropLocation;

        public InventoryComponent(GameEntity entity, IPositionalItemStorage itemDropLocation) : base(entity) {
            _itemDropLocation = itemDropLocation;
        }

        public override void Update(float delta) { }
        
        // When an entity dies, all its items are dropped nearby on the floor
        public void Message(DiedMessage message) {
            Inventory.DropAllItems(_itemDropLocation, Entity.Position);
        }
    }
}

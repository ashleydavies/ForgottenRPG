using ShaRPG.Entity.Components.Messages;
using ShaRPG.Items;
using ShaRPG.Map;

namespace ShaRPG.Entity.Components {
    public class InventoryComponent : AbstractComponent, IMessageHandler<DiedMessage> {
        public readonly Inventory Inventory = new Inventory();
        
        private readonly IPositionalItemStorage _itemDropLocation;

        public InventoryComponent(GameEntity entity, IPositionalItemStorage itemDropLocation) : base(entity) {
            _itemDropLocation = itemDropLocation;
        }

        public override void Update(float delta) { }
        
        // When an entity dies, all its items are dropped nearby on the floor
        public void Message(DiedMessage message) {
            Inventory.DropAllItems(_itemDropLocation, _entity.Position);
        }
    }
}

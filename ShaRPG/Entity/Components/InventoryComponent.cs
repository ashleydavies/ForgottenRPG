using ShaRPG.Items;

namespace ShaRPG.Entity.Components {
    public class InventoryComponent : AbstractComponent {
        public Inventory Inventory = new Inventory();

        public InventoryComponent(GameEntity entity) : base(entity) {
            
        }

        public override void Update(float delta) {
            
        }
    }
}

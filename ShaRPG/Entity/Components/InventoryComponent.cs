using ShaRPG.Items;

namespace ShaRPG.Entity.Components {
    public class InventoryComponent : AbstractComponent {
        private Inventory _inventory = new Inventory();

        public InventoryComponent(GameEntity entity) : base(entity) {
            
        }

        public override void Update(float delta) {
            
        }
    }
}

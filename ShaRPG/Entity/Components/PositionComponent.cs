#region

using System;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Entity.Components {
    public class PositionComponent : AbstractComponent {
        private GameCoordinate Position { get; set; }

        public PositionComponent(Entity entity) : base(entity) { }
        
        public void Move(GameCoordinate offset) {
            Position += offset;
        }

        public void SetPosition(GameCoordinate position) {
            Position = position;
        }

        public GameCoordinate GetPosition() {
            return Position;
        }

        public override void Update() { }

        public override void Message(IComponentMessage componentMessage) { }
    }
}

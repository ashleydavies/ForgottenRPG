using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class MouseClickMessage : IComponentMessage {
        public readonly GameCoordinate MousePosition;
        
        public MouseClickMessage(GameCoordinate mousePosition) {
            MousePosition = mousePosition;
        }
    }
}

using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components.Messages {
    public class PlayerInteractMessage : IComponentMessage {
        public readonly GameCoordinate MousePosition;
        
        public PlayerInteractMessage(GameCoordinate mousePosition) {
            MousePosition = mousePosition;
        }
    }
}

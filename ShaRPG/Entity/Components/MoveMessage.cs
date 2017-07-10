using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class MoveMessage : IComponentMessage {
        public readonly TileCoordinate DesiredPosition;

        public MoveMessage(TileCoordinate coordinates) {
            DesiredPosition = coordinates;
        }
    }
}

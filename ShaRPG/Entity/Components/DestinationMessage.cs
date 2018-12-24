using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class DestinationMessage : IComponentMessage {
        public readonly TileCoordinate DesiredPosition;

        public DestinationMessage(TileCoordinate coordinates) {
            DesiredPosition = coordinates;
        }
    }
}

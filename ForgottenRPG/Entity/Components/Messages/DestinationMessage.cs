using ForgottenRPG.Util.Coordinate;

namespace ForgottenRPG.Entity.Components.Messages {
    public class DestinationMessage : IComponentMessage {
        public readonly TileCoordinate DesiredPosition;

        public DestinationMessage(TileCoordinate coordinates) {
            DesiredPosition = coordinates;
        }
    }
}

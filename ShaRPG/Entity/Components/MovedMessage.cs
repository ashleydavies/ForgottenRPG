using System.ComponentModel.DataAnnotations;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class MovedMessage : IComponentMessage {
        public readonly TileCoordinate OldPosition;
        public readonly TileCoordinate NewPosition;

        public MovedMessage(TileCoordinate old, TileCoordinate @new) {
            OldPosition = old;
            NewPosition = @new;
        }
    }
}

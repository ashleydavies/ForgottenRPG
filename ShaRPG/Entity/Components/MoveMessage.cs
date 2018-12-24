using System.ComponentModel.DataAnnotations;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Entity.Components {
    public class MoveMessage : IComponentMessage {
        public readonly TileCoordinate OldPosition;
        public readonly TileCoordinate NewPosition;

        public MoveMessage(TileCoordinate old, TileCoordinate @new) {
            OldPosition = old;
            NewPosition = @new;
        }
    }
}

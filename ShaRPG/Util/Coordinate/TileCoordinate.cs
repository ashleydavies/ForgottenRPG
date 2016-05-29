#region

using ShaRPG.Map;

#endregion

namespace ShaRPG.Util.Coordinate {
    public class TileCoordinate : Coordinate {
        public TileCoordinate(int x, int y) : base(x, y) {}

        public static implicit operator GameCoordinate(TileCoordinate tileCoordinate) {
            return new GameCoordinate(
                (int) ((tileCoordinate.X - tileCoordinate.Y) / 2.0 * MapTile.Width),
                (int) ((tileCoordinate.X + tileCoordinate.Y) / 2.0 * MapTile.Height)
                );
        }
    }
}

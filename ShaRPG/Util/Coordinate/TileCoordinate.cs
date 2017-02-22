#region

using System;
using ShaRPG.Map;

#endregion

namespace ShaRPG.Util.Coordinate {
    public class TileCoordinate : Coordinate {
        public TileCoordinate(int x, int y) : base(x, y) { }

        public static implicit operator GameCoordinate(TileCoordinate tileCoordinate) {
            return new GameCoordinate(
                (int) ((tileCoordinate.X - tileCoordinate.Y) / 2.0 * MapTile.Width),
                (int) ((tileCoordinate.X + tileCoordinate.Y) / 2.0 * MapTile.Height)
            );
        }

        public static TileCoordinate operator -(TileCoordinate a, TileCoordinate b) {
            return new TileCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public int ManhattanDistance(TileCoordinate other) {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public override bool Equals(object obj) {
            if (!(obj is TileCoordinate)) return false;

            TileCoordinate other = (TileCoordinate) obj;
            return other.X == X && other.Y == Y;
        }

    }
}

using System;
using ForgottenRPG.Map;

namespace ForgottenRPG.Util.Coordinate {
    public readonly struct TileCoordinate {
        public readonly int X;
        public readonly int Y;

        public TileCoordinate(int x, int y) {
            X = x;
            Y = y;
        }

        public static GameCoordinate IsoToCartesian(int x, int y) {
            return new GameCoordinate(
                (x / MapTile.HalfWidth - y / MapTile.HalfWidth) * MapTile.HalfWidth,
                (x / MapTile.HalfWidth + y / MapTile.HalfWidth) * MapTile.HalfHeight
            );
        }

        public static implicit operator GameCoordinate(TileCoordinate tileCoordinate) {
            return IsoToCartesian(tileCoordinate.X * MapTile.HalfWidth, tileCoordinate.Y * MapTile.HalfWidth);
        }
        
        public static implicit operator TileCoordinate(GameCoordinate gameCoordinate) {
            return new TileCoordinate(
                (int) Math.Floor(gameCoordinate.X / (float) MapTile.Width + gameCoordinate.Y / (float) MapTile.Height),
                (int) Math.Floor(gameCoordinate.Y / (float) MapTile.Height - gameCoordinate.X / (float) MapTile.Width)
            );
        }

        public static TileCoordinate operator +(TileCoordinate a, TileCoordinate b) {
            return new TileCoordinate(a.X + b.X, a.Y + b.Y);
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
        
        public override string ToString() {
            return $"<{X}, {Y}>";
        }
    }
}

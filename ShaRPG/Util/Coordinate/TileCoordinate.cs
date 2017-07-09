using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ShaRPG.Map;
using ShaRPG.Service;

namespace ShaRPG.Util.Coordinate {
    public class TileCoordinate : Coordinate {
        public TileCoordinate(int x, int y) : base(x, y) { }

        public static implicit operator GameCoordinate(TileCoordinate tileCoordinate) {
            return new GameCoordinate(
                (tileCoordinate.X - tileCoordinate.Y) * MapTile.HalfWidth,
                (tileCoordinate.X + tileCoordinate.Y) * MapTile.HalfHeight
            );
        }
        
        public static implicit operator TileCoordinate(GameCoordinate gameCoordinate) {
            return new TileCoordinate(
                (int) Math.Floor(gameCoordinate.X / 64.0 + gameCoordinate.Y / 32.0),
                (int) Math.Floor(gameCoordinate.Y / 32.0 - gameCoordinate.X / 64.0)
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
    }
}

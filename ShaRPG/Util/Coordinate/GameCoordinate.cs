using System;

namespace ShaRPG.Util.Coordinate {
    public class GameCoordinate : Coordinate {
        public GameCoordinate(int x, int y) : base(x, y) {}

        public static GameCoordinate operator +(GameCoordinate a, GameCoordinate b) {
            return new GameCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static GameCoordinate operator -(GameCoordinate a, GameCoordinate b) {
            return new GameCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static GameCoordinate operator /(GameCoordinate a, float n) {
            return new GameCoordinate((int) (a.X / n), (int) (a.Y / n));
        }

        public static GameCoordinate operator *(GameCoordinate a, float n) {
            return new GameCoordinate((int) (a.X * n), (int) (a.Y * n));
        }
    }
}

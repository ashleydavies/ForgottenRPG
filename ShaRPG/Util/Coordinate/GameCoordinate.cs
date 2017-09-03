using System;
using SFML.System;

namespace ShaRPG.Util.Coordinate {
    public class GameCoordinate : Coordinate {
        public GameCoordinate(int x, int y) : base(x, y) { }

        public static implicit operator Vector2I(GameCoordinate coordinate) {
            return new Vector2I(coordinate.X, coordinate.Y);
        }
        
        public static implicit operator Vector2F(GameCoordinate coordinate) {
            return new Vector2F(coordinate.X, coordinate.Y);
        }
        
        public static implicit operator Vector2f(GameCoordinate coordinate) {
            return new Vector2f(coordinate.X, coordinate.Y);
        }
        
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
        
        public double EuclideanDistance(GameCoordinate other) {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public bool Overlaps(GameCoordinate position, int width, int height) {
            return X > position.X && Y > position.Y && X < position.X + width && Y < position.Y + height;
        }
    }
}

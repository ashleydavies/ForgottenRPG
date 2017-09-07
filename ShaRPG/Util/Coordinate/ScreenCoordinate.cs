using SFML.System;

namespace ShaRPG.Util.Coordinate {
    public class ScreenCoordinate : Coordinate {
        public ScreenCoordinate(int x, int y) : base(x, y) { }

        public ScreenCoordinate(Vector2i v) : base(v.X, v.Y) { }
        
        public static implicit operator Vector2f(ScreenCoordinate coordinate) {
            return new Vector2f(coordinate.X, coordinate.Y);
        }

        public GameCoordinate AsGameCoordinate(Vector2f offset) {
            return new GameCoordinate((int) (offset.X + X), (int) (offset.Y + Y));
        }
        
        public static ScreenCoordinate operator +(ScreenCoordinate a, ScreenCoordinate b) {
            return new ScreenCoordinate(a.X + b.X, a.Y + b.Y);
        }
        
        public static ScreenCoordinate operator -(ScreenCoordinate a, ScreenCoordinate b) {
            return new ScreenCoordinate(a.X - b.X, a.Y - b.Y);
        }
        
        public static ScreenCoordinate operator /(ScreenCoordinate a, int scalar) {
            return new ScreenCoordinate(a.X / scalar, a.Y / scalar);
        }

        public bool Overlaps(ScreenCoordinate position, int width, int height) {
            return X > position.X && Y > position.Y && X < position.X + width && Y < position.Y + height;
        }
    }
}

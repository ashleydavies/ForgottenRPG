using SFML.System;

namespace ForgottenRPG.Util.Coordinate {
    public readonly struct ScreenCoordinate {
        public readonly int X;
        public readonly int Y;

        public ScreenCoordinate(int x, int y) {
            X = x;
            Y = y;
        }

        public ScreenCoordinate(Vector2i v) : this(v.X, v.Y) { }
        public ScreenCoordinate(Vector2f v) : this((int) v.X, (int) v.Y) { }
        
        public static implicit operator Vector2f(ScreenCoordinate coordinate) {
            return new Vector2f(coordinate.X, coordinate.Y);
        }
        
        public GameCoordinate AsGameCoordinate(Vector2f offset, float scale) {
            return new GameCoordinate((int) (offset.X + X), (int) (offset.Y + Y));
        }
        
        public static ScreenCoordinate operator +(ScreenCoordinate a, ScreenCoordinate b) {
            return new ScreenCoordinate(a.X + b.X, a.Y + b.Y);
        }
        
        public static ScreenCoordinate operator -(ScreenCoordinate a, ScreenCoordinate b) {
            return new ScreenCoordinate(a.X - b.X, a.Y - b.Y);
        }
        
        public static ScreenCoordinate operator *(ScreenCoordinate a, float mul) {
            return new ScreenCoordinate((int) (a.X * mul), (int) (a.Y * mul));
        }
        
        public static ScreenCoordinate operator /(ScreenCoordinate a, float div) {
            return new ScreenCoordinate((int) (a.X / div), (int) (a.Y / div));
        }
        
        public static ScreenCoordinate operator /(ScreenCoordinate a, int scalar) {
            return new ScreenCoordinate(a.X / scalar, a.Y / scalar);
        }

        public bool Overlaps(ScreenCoordinate position, int width, int height) {
            return X > position.X && Y > position.Y && X < position.X + width && Y < position.Y + height;
        }
        
        public override string ToString() {
            return $"<{X}, {Y}>";
        }
    }
}

using SFML.System;

namespace ShaRPG.Util {
    public class Vector2I {
        public int X;
        public int Y;

        public Vector2I(int x, int y) {
            X = x;
            Y = y;
        }
        
        public static implicit operator Vector2I(Vector2u vector2U) {
            return new Vector2I((int) vector2U.X, (int) vector2U.Y);
        }

        public static Vector2I operator +(Vector2I left, Vector2I right) {
            return new Vector2I(left.X + right.X, left.Y + right.Y);
        }
        
        public static Vector2I operator -(Vector2I left, Vector2I right) {
            return new Vector2I(left.X - right.X, left.Y - right.Y);
        }
        
        public static Vector2I operator /(Vector2I dividend, int divisor) {
            return new Vector2I(dividend.X / divisor, dividend.Y / divisor);
        }

        public override string ToString() {
            return $"<{X}, {Y}>";
        }
    }
}

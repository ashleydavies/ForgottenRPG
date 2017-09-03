using System;

namespace ShaRPG.Util {
    public class Vector2F {
        public float X;
        public float Y;

        public Vector2F(float x, float y) {
            X = x;
            Y = y;
        }

        public static explicit operator Vector2I(Vector2F vector2F) {
            return new Vector2I((int) Math.Round(vector2F.X), (int) Math.Round(vector2F.Y));
        }

        public static implicit operator Vector2F(Vector2I other) {
            return new Vector2F(other.X, other.Y);
        }
        
        public static Vector2F operator +(Vector2F left, Vector2F right) {
            return new Vector2F(left.X + right.X, left.Y + right.Y);
        }
        
        public static Vector2F operator -(Vector2F left, Vector2F right) {
            return new Vector2F(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2F operator *(Vector2F left, float right) {
            return new Vector2F(left.X * right, left.Y * right);
        }

        public override string ToString() {
            return $"<{X}, {Y}>";
        }
    }
}

namespace ShaRPG.Util {
    public class Vector2F {
        public float X;
        public float Y;

        public Vector2F(float x, float y) {
            X = x;
            Y = y;
        }

        public static Vector2F operator +(Vector2F left, Vector2F right) {
            return new Vector2F(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2F operator *(Vector2F left, float right) {
            return new Vector2F(left.X * right, left.Y * right);
        }
    }
}

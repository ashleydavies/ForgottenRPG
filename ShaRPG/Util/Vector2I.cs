namespace ShaRPG.Util {
    public class Vector2I {
        public int X;
        public int Y;

        public Vector2I(int x, int y) {
            X = x;
            Y = y;
        }

        public static Vector2I operator +(Vector2I left, Vector2I right) {
            return new Vector2I(left.X + right.X, left.Y + right.Y);
        }
    }
}

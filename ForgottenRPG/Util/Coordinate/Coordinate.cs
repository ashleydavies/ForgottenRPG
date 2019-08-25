namespace ForgottenRPG.Util.Coordinate {
    public abstract class Coordinate {
        public readonly int X;
        public readonly int Y;

        protected Coordinate(int x, int y) {
            X = x;
            Y = y;
        }

        public override string ToString() {
            return $"<{X}, {Y}>";
        }
    }
}

using ShaRPG.Camera;

namespace ShaRPG.Util.Coordinate {
    public class ScreenCoordinate : Coordinate {
        public ScreenCoordinate(int x, int y) : base(x, y) { }

        public ScreenCoordinate(Vector2I v) : base(v.X, v.Y) { }

        public GameCoordinate AsGameCoordinate(ICamera camera) {
            return camera.TranslateScreenCoordinate(this);
        }
        
        public static ScreenCoordinate operator +(ScreenCoordinate a, ScreenCoordinate b) {
            return new ScreenCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public bool Overlaps(ScreenCoordinate position, int width, int height) {
            return X > position.X && Y > position.Y && X < position.X + width && Y < position.Y + height;
        }
    }
}

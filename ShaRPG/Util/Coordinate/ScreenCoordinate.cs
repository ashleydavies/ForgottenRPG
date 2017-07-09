using ShaRPG.Camera;

namespace ShaRPG.Util.Coordinate {
    public class ScreenCoordinate : Coordinate {
        public ScreenCoordinate(int x, int y) : base(x, y) { }

        public GameCoordinate AsGameCoordinate(ICamera camera) {
            return camera.TranslateScreenCoordinate(this);
        } 
    }
}

using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Camera {
    public interface ICamera {
        Vector2F Scale { get; set; }
        Vector2F Center { get; set; }
        GameCoordinate TranslateScreenCoordinate(ScreenCoordinate screenCoordinate);
    }
}

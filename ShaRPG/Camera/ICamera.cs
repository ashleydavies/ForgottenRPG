using ShaRPG.Util;

namespace ShaRPG.Camera {
    public interface ICamera {
        Vector2F Scale { get; set; }
        Vector2F Center { get; set; }
    }
}

using ShaRPG.Util;

namespace ShaRPG.Camera {
    internal class GameCamera : ICamera {
        public Vector2F Scale { get; set; } = new Vector2F(1, 1);
        public Vector2F Center { get; set; } = new Vector2F(0, 0);
    }
}

#region

using ShaRPG.Util;

#endregion

namespace ShaRPG.Camera {
    internal class GameCamera : ICamera {
        public Vector2F Scale { get; set; } = new Vector2F(1, 1);
        public Vector2I Center { get; set; } = new Vector2I(0, 0);
    }
}

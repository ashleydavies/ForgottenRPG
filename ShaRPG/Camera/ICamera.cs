#region

using ShaRPG.Util;

#endregion

namespace ShaRPG.Camera {
    public interface ICamera {
        Vector2F Scale { get; set; }
        Vector2F Center { get; set; }
    }
}

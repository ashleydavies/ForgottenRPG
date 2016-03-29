using ShaRPG.Util;

namespace ShaRPG.Camera
{
    public interface ICamera
    {
        Vector2I Center { get; set; }
        Vector2I Size { get; set; }
    }
}
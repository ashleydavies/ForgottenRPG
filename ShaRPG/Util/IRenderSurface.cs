#region

using ShaRPG.Camera;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Util {
    public interface IRenderSurface {
        Vector2I Size { get; set; }
        void Render(IDrawable sprite, GameCoordinate position);
        void SetCamera(ICamera gameCamera);
    }
}

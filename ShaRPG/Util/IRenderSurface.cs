using ShaRPG.Camera;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public interface IRenderSurface {
        Vector2I Size { get; set; }
        void Render(IDrawable drawable, GameCoordinate position);
        void Render(IDrawable drawable, ScreenCoordinate position);
        void AddRenderOffset(Vector2I offset);
        void SubtractRenderOffset(Vector2I offset);
        void SetCamera(ICamera gameCamera);
    }
}

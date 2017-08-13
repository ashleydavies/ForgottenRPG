using ShaRPG.Camera;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public interface IRenderSurface {
        Vector2I Size { get; set; }
        void Render(IDrawable sprite, GameCoordinate position);
        void Render(IDrawable sprite, ScreenCoordinate position);
        void AddRenderOffset(Vector2I offset);
        void SubtractRenderOffset(Vector2I offset);
        void SetCamera(ICamera gameCamera);
    }
}

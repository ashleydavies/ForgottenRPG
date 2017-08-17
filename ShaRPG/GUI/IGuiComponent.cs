using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public interface IGuiComponent : IClickObserver {
        IGuiComponentContainer Parent { get; set; }
        ScreenCoordinate ScreenPosition { get; }
        int Height { get; }
        int Width { get; }
        void Render(IRenderSurface renderSurface);
        void Reflow();
    }
}

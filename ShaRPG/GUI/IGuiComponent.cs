using SFML.Graphics;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public interface IGuiComponent : IClickObserver {
        IGuiComponentContainer Parent { get; set; }
        ScreenCoordinate ScreenPosition { get; }
        int Height { get; }
        int Width { get; }
        void Render(RenderTarget renderSurface);
        void Reflow();
    }
}

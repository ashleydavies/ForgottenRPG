using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.GUI {
    public interface IGuiComponent : IClickObserver {
        IGuiComponentContainer Parent { get; set; }
        ScreenCoordinate ScreenPosition { get; }
        int Height { get; }
        int Width { get; }
        void Render(RenderTarget renderSurface);
        void Reflow();
    }
}

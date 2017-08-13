using ShaRPG.Util;

namespace ShaRPG.GUI {
    public interface IGuiComponent {
        int Height { get; }
        int Width { get; }
        void Render(IRenderSurface renderSurface);
    }
}

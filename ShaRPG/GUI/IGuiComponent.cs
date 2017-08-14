using ShaRPG.Util;

namespace ShaRPG.GUI {
    public interface IGuiComponent {
        IGuiComponentContainer Parent { get; set; }
        int Height { get; }
        int Width { get; }
        void Render(IRenderSurface renderSurface);
        void Reflow();
    }
}

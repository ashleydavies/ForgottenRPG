using ShaRPG.Util;

namespace ShaRPG.Entity.Components {
    public interface IComponent {
        void Update(float delta);
        void Render(IRenderSurface renderSurface);
    }
}

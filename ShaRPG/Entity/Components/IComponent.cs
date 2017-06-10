using ShaRPG.Util;

namespace ShaRPG.Entity.Components {
    public interface IComponent {
        void Update(float delta);
        void Message(IComponentMessage componentMessage);
        void Render(IRenderSurface renderSurface);
    }
}

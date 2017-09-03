using SFML.Graphics;

namespace ShaRPG.Entity.Components {
    public interface IComponent {
        void Update(float delta);
        void Render(RenderTarget renderSurface);
    }
}

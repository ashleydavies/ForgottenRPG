using SFML.Graphics;

namespace ForgottenRPG.Entity.Components {
    public interface IComponent {
        void Update(float delta);
        void Render(RenderTarget renderSurface);
    }
}

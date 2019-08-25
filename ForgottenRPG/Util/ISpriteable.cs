using SFML.Graphics;

namespace ForgottenRPG.Util {
    public interface ISpriteable {
        Sprite Sprite { get; }
        void Update(float delta);
    }
}

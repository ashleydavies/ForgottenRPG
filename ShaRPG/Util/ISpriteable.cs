using SFML.Graphics;

namespace ShaRPG.Util {
    public interface ISpriteable {
        Sprite Sprite { get; }
        void Update(float delta);
    }
}

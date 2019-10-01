using SFML.Graphics;

namespace ForgottenRPG.Util {
    public class SpriteableWrapper : ISpriteable {
        public SpriteableWrapper(Sprite sprite) {
            Sprite = sprite;
        }
        
        public Sprite Sprite { get; }
        public void Update(float delta) {}
    }
}

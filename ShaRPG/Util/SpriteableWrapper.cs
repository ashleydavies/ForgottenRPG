using SFML.Graphics;

namespace ShaRPG.Util {
    public class SpriteableWrapper : ISpriteable {
        public SpriteableWrapper(Sprite sprite) {
            Sprite = sprite;
        }
        
        public Sprite Sprite { get; }
        public void Update(float delta) {}
    }
}

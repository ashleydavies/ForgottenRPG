using Sprite = SFML.Graphics.Sprite;

namespace ShaRPG.Service {
    public interface ITextureStore {
        Sprite GetNewSprite(string name);
    }
}

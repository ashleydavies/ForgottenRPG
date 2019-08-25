using Sprite = SFML.Graphics.Sprite;

namespace ForgottenRPG.Service {
    public interface ITextureStore {
        Sprite GetNewSprite(string name);
    }
}

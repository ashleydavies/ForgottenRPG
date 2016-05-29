#region

using ShaRPG.Util;

#endregion

namespace ShaRPG.Service {
    public interface ISpriteStoreService {
        Sprite GetSprite(string name);
    }
}

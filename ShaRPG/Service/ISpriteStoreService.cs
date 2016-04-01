using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Util;

namespace ShaRPG.Service {
    public interface ISpriteStoreService
    {
        Sprite GetSprite(string name);
    }
}

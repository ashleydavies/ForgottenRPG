using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace ShaRPG.Util {
    class Sprite
    {
        public readonly SFML.Graphics.Sprite UnderlyingSprite;

        public Sprite(Texture texture, int x, int y, int width, int height)
        {
            UnderlyingSprite = new SFML.Graphics.Sprite
            {
                Texture = texture.UnderlyingTexture,
                TextureRect = new IntRect(x, y, width, height)
            };
        }
    }
}

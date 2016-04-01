using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using ShaRPG.Service;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class Sprite : IDrawable
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

        public void Draw(RenderWindow window, GameCoordinate position)
        {
            UnderlyingSprite.Position = new Vector2f(position.X, position.Y);
            window.Draw(UnderlyingSprite);
        }

        public void Update(float delta)
        {

        }
    }
}

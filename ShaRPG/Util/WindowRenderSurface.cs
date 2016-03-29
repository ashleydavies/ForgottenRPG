using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace ShaRPG.Util {
    class WindowRenderSurface : IRenderSurface
    {
        private readonly RenderWindow _window;

        public WindowRenderSurface(RenderWindow window)
        {
            _window = window;
        }

        public void Render(Sprite sprite)
        {
            _window.Draw(sprite.UnderlyingSprite);
        }
    }
}

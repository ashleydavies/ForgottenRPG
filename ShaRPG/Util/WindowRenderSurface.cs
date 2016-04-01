using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using ShaRPG.Camera;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    class WindowRenderSurface : IRenderSurface
    {
        public Vector2I Size { get; set; }
        private readonly RenderWindow _window;

        public WindowRenderSurface(RenderWindow window)
        {
            _window = window;
            Size = new Vector2I((int)_window.Size.X, (int)_window.Size.Y);
        }

        public void Render(IDrawable sprite, GameCoordinate position)
        {
            sprite.Draw(_window, position);
        }

        public void SetCamera(ICamera gameCamera) => _window.SetView(new View(
            new Vector2f(gameCamera.Center.X, gameCamera.Center.Y), new Vector2f(Size.X / 2, Size.Y / 2)
            ));
    }
}

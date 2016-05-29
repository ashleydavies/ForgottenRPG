#region

using SFML.Graphics;
using SFML.System;
using ShaRPG.Camera;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Util {
    internal class WindowRenderSurface : IRenderSurface {
        private readonly RenderWindow _window;

        public WindowRenderSurface(RenderWindow window) {
            _window = window;
            Size = new Vector2I((int) _window.Size.X, (int) _window.Size.Y);
        }

        public Vector2I Size { get; set; }

        public void Render(IDrawable sprite, GameCoordinate position) {
            sprite.Draw(_window, position);
        }

        public void SetCamera(ICamera gameCamera) => _window.SetView(new View(
                                                                         new Vector2f(gameCamera.Center.X,
                                                                                      gameCamera.Center.Y),
                                                                         new Vector2f(Size.X / gameCamera.Scale.X,
                                                                                      Size.Y / gameCamera.Scale.Y)
                                                                         ));
    }
}

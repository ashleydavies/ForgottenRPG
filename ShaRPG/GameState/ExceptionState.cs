using System;
using SFML.Graphics;
using SFML.System;
using ShaRPG.GUI;
using ShaRPG.Service;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState
{
    public class ExceptionState : AbstractGameState {
        private readonly GuiWindow _exceptionGuiWindow;
        private static readonly Vector2i GuiWindowSize = new Vector2i(60 * 9, 60 * 12);

        public ExceptionState(Game game, Exception exception, Vector2f windowSize, ITextureStore textureStore)
            : base(game) {
            _exceptionGuiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), GuiWindowSize);

            var contents = $"Encountered exception. Details:" +
                           $"Type: {exception.GetType()}" +
                           $"Message: {exception.Message}";
            var inner = new PaddingContainer(20);
            inner.AddComponent(new TextContainer(contents, 18));
            _exceptionGuiWindow.AddComponent(inner);
            Console.WriteLine(contents);
        }

        public override void Update(float delta) {
        }

        public override void Render(RenderTarget renderSurface) {
            _exceptionGuiWindow.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            if (_exceptionGuiWindow.IsMouseOver(coordinates)) _exceptionGuiWindow.Clicked(coordinates);
        }

        public override void MouseWheelMoved(float delta) { }
    }
}

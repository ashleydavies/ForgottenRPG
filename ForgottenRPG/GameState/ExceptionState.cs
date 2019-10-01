using System;
using ForgottenRPG.GUI;
using ForgottenRPG.Service;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;

namespace ForgottenRPG.GameState
{
    public class ExceptionState : AbstractGameState {
        private readonly GuiWindow _exceptionGuiWindow;
        private static readonly Vector2i GuiWindowSize = new Vector2i(60 * 12, 60 * 14);

        public ExceptionState(Game game, Exception exception, Vector2f windowSize, ITextureStore textureStore)
            : base(game) {
            _exceptionGuiWindow = new GuiWindow(textureStore, (Vector2i) (windowSize / 2), GuiWindowSize);

            var contents =
                $"EXCEPTION\n\n Type: {exception.GetType()}\n Message: {exception.Message}";
            ServiceLocator.LogService.Log(LogType.Error, contents);
            
            var inner = new PaddingContainer(20, new TextContainer(contents, 26));
            _exceptionGuiWindow.AddComponent(inner);
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

using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.GameState {
    public abstract class AbstractGameState {
        protected Game Game;

        protected AbstractGameState(Game game) {
            Game = game;
        }

        public abstract void Update(float delta);
        public abstract void Render(RenderTarget renderSurface);

        public void SetState(AbstractGameState state) {
            Game.SetGameState(state);
        }

        public void ChangeState(AbstractGameState state) {
            Game.ChangeGameState(state);
        }

        public void EndState() {
            Game.EndGameState();
        }

        public abstract void Clicked(ScreenCoordinate coordinates);
        public abstract void MouseWheelMoved(float delta);
    }
}

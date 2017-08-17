using ShaRPG.Camera;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    public abstract class AbstractGameState {
        protected Game Game;

        protected AbstractGameState(Game game, ICamera camera) {
            Game = game;
            Camera = camera;
        }

        public ICamera Camera { get; protected set; }
        public abstract void Update(float delta);
        public abstract void Render(IRenderSurface renderSurface);

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
        public abstract void MouseWheelMoved(int delta);
    }
}

using ShaRPG.Camera;
using ShaRPG.Util;

namespace ShaRPG.GameState {
    public abstract class AbstractGameState {
        protected Game Game;

        protected AbstractGameState(Game game) {
            Game = game;
        }

        public ICamera Camera { get; protected set; }
        public abstract void Update(float delta);
        public abstract void Render(IRenderSurface renderSurface);

        public void ChangeState(AbstractGameState state) {
            Game.SetGameState(state);
        }

        public abstract void MouseWheelMoved(int delta);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Camera;
using ShaRPG.Util;

namespace ShaRPG.GameState {
    public abstract class AbstractGameState
    {
        public ICamera Camera { get; protected set; }
        protected Game Game;

        protected AbstractGameState(Game game)
        {
            Game = game;
        }

        public abstract void Update(float delta);
        public abstract void Draw(IRenderSurface renderSurface);

        public void ChangeState(AbstractGameState state)
        {
            Game.SetGameState(state);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Util;

namespace ShaRPG.GameState {
    abstract class AbstractGameState
    {
        public abstract void Update(Game game, int delta);
        public abstract void Draw(Game game, IRenderSurface renderSurface);
        public abstract void ChangeState(Game game, AbstractGameState state);
    }
}

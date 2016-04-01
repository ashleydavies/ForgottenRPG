using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaRPG.Camera;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    class GameState : AbstractGameState
    {
        private readonly GameMap _map;

        public GameState(Game game, ISpriteStoreService spriteStore, MapTileStore mapTileStore) : base(game)
        {
            Camera = new GameCamera();
            _map = GameMap.FromXml(mapTileStore);
        }

        public override void Update(float delta)
        {
            _map.Update(delta);
        }
        
        public override void Draw(IRenderSurface renderSurface)
        {
            for (var x = 0; x < _map.Size.X; x++)
            {
                for (var y = _map.Size.Y - 1; y >= 0; y--)
                {
                    _map.GetTile(new TileCoordinate(x, y)).Draw(renderSurface, new TileCoordinate(x, y));
                }
            }
        }
    }
}

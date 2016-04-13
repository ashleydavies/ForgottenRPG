using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;
using ShaRPG.Camera;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    class GameState : AbstractGameState
    {
        private readonly GameMap _map;
        private readonly MapLoader _mapLoader;

        public GameState(Game game, ISpriteStoreService spriteStore, MapTileStore mapTileStore) : base(game)
        {
            Camera = new GameCamera();
            _mapLoader = new MapLoader(Path.Combine("resources", "data", "xml", "map"), mapTileStore);
            _map = _mapLoader.LoadMap(0);
        }

        public override void Update(float delta)
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                Camera.Center += new Vector2I(0, -10);
            } else if ( Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                Camera.Center += new Vector2I(0, 10);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                Camera.Center += new Vector2I(-10, 0);
            } else if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                Camera.Center += new Vector2I(10, 0);
            }

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

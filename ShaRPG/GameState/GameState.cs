using System.Collections.Generic;
using System.IO;
using SFML.Window;
using ShaRPG.Camera;
using ShaRPG.Command;
using ShaRPG.Entity;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    internal class GameState : AbstractGameState {
        private readonly GameMap _map;
        private readonly MapLoader _mapLoader;
        private readonly EntityManager _entityManager;
        private readonly Entity.Entity _player;
        private readonly Dictionary<Keyboard.Key, ICommand> _keyMappings;

        public GameState(Game game, ISpriteStoreService spriteStore, MapTileStore mapTileStore) : base(game) {
            Camera = new GameCamera();
            _mapLoader = new MapLoader(Path.Combine("resources", "data", "xml", "map"), mapTileStore);
            _map = _mapLoader.LoadMap(0);
            _entityManager = new EntityManager();

            _player = new Entity.Entity(_entityManager, "Player", 100, new Vector2I(0, 0), _map);

            _keyMappings = new Dictionary<Keyboard.Key, ICommand> {
                {Keyboard.Key.Up, new CameraMoveCommand(Camera, new Vector2F(0, -50))},
                {Keyboard.Key.Down, new CameraMoveCommand(Camera, new Vector2F(0, 50))},
                {Keyboard.Key.Left, new CameraMoveCommand(Camera, new Vector2F(-50, 0))},
                {Keyboard.Key.Right, new CameraMoveCommand(Camera, new Vector2F(50, 0))}
            };
        }

        public override void Update(float delta) {
            foreach (Keyboard.Key key in _keyMappings.Keys) {
                if (Keyboard.IsKeyPressed(key)) {
                    _keyMappings[key].Execute(delta);
                }
            }

            _map.Update(delta);
            _entityManager.Update(delta);
        }

        public override void Render(IRenderSurface renderSurface) {
            _map.Render(renderSurface);
            _entityManager.Render(renderSurface);
        }

        public override void MouseWheelMoved(int delta) {
            ServiceLocator.LogService.Log(LogType.Information, Camera.Scale.X.ToString());
            Camera.Scale.X += delta / 10f;
            Camera.Scale.Y += delta / 10f;
        }
    }
}

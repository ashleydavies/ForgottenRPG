using System.Collections.Generic;
using System.IO;
using SFML.Window;
using ShaRPG.Camera;
using ShaRPG.Command;
using ShaRPG.Entity;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;

namespace ShaRPG.GameState {
    public class GameState : AbstractGameState {
        private GameEntity _player => _entityManager.Player;
        private readonly GameMap _map;
        private readonly MapLoader _mapLoader;
        private readonly EntityLoader _entityLoader;
        private readonly EntityManager _entityManager;
        private readonly Dictionary<Keyboard.Key, ICommand> _keyMappings;

        public GameState(Game game, ISpriteStoreService spriteStore, MapTileStore mapTileStore) : base(game) {
            Camera = new GameCamera();
            _entityManager = new EntityManager();
            _entityLoader = new EntityLoader(Config.EntityDataDirectory, _entityManager, spriteStore);
            _mapLoader = new MapLoader(Config.MapDataDirectory, mapTileStore);
            _map = _mapLoader.LoadMap(0);
            _map.SpawnEntities(_entityLoader);

            _keyMappings = new Dictionary<Keyboard.Key, ICommand> {
                {Keyboard.Key.Up, new CameraMoveCommand(Camera, new Vector2F(0, -300))},
                {Keyboard.Key.Down, new CameraMoveCommand(Camera, new Vector2F(0, 300))},
                {Keyboard.Key.Left, new CameraMoveCommand(Camera, new Vector2F(-300, 0))},
                {Keyboard.Key.Right, new CameraMoveCommand(Camera, new Vector2F(300, 0))},
                {Keyboard.Key.X, new ExitGameCommand(this)}
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

        public void ExitGame() {
            throw new EndGameException();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using SFML.Window;
using ShaRPG.Camera;
using ShaRPG.Command;
using ShaRPG.Entity;
using ShaRPG.Entity.Components;
using ShaRPG.EntityDialog;
using ShaRPG.GUI;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GameState {
    public class StateGame : AbstractGameState, IOpenDialog {
        private GameEntity _player => _entityManager.Player;
        private readonly GameMap _map;
        private readonly MapLoader _mapLoader;
        private readonly EntityLoader _entityLoader;
        private readonly EntityManager _entityManager;
        private readonly ClickManager _clickManager = new ClickManager();
        private readonly Dictionary<Keyboard.Key, ICommand> _keyMappings;
        private readonly FPSCounter _fpsCounter = new FPSCounter();
        private readonly ISpriteStoreService _spriteStore;
        private Vector2I _windowSize;

        public StateGame(Game game, Vector2I size, ISpriteStoreService spriteStore,
                         MapTileStore mapTileStore) : base(game, new GameCamera(size)) {
            _windowSize = size;
            _spriteStore = spriteStore;
            _entityManager = new EntityManager(this);
            _entityLoader = new EntityLoader(Config.EntityDataDirectory, _entityManager, spriteStore);
            _mapLoader = new MapLoader(Config.MapDataDirectory, mapTileStore);
            _map = _mapLoader.LoadMap(0, this);
            _map.SpawnEntities(_entityLoader);
            _clickManager.Register(ClickPriority.Entity, _entityManager);
            _clickManager.Register(ClickPriority.Map, _map);

            _keyMappings = new Dictionary<Keyboard.Key, ICommand> {
                {
                    Keyboard.Key.Up, new CameraMoveCommand(Camera, new Vector2F(0, -300))
                }, {
                    Keyboard.Key.Down, new CameraMoveCommand(Camera, new Vector2F(0, 300))
                }, {
                    Keyboard.Key.Left, new CameraMoveCommand(Camera, new Vector2F(-300, 0))
                }, {
                    Keyboard.Key.Right, new CameraMoveCommand(Camera, new Vector2F(300, 0))
                }, {
                    Keyboard.Key.X, new ExitGameCommand(this)
                }
            };

            if (_player == null) throw new EntityException("No player was created during map loading time");

            Camera.Center = (GameCoordinate) _player.Position;
        }

        public override void Update(float delta) {
            foreach (Keyboard.Key key in _keyMappings.Keys) {
                if (Keyboard.IsKeyPressed(key)) {
                    _keyMappings[key].Execute(delta);
                }
            }

            _map.Update(delta);
            _entityManager.Update(delta);
            _fpsCounter.Update(delta);
        }

        public override void Render(IRenderSurface renderSurface) {
            _map.Render(renderSurface);
            _entityManager.Render(renderSurface);
            _fpsCounter.Render(renderSurface);
        }

        public override void Clicked(ScreenCoordinate coordinates) {
            _clickManager.Clicked(coordinates);
        }

        public override void MouseWheelMoved(int delta) {
            ServiceLocator.LogService.Log(LogType.Information, Camera.Scale.X.ToString());
            Camera.Scale.X += delta / 10f;
            Camera.Scale.Y += delta / 10f;
        }

        public void MovePlayer(TileCoordinate destination) {
            _player.SendMessage(new MoveMessage(destination));
        }

        public void ExitGame() {
            throw new EndGameException();
        }

        public GameCoordinate TranslateCoordinates(ScreenCoordinate coordinates) {
            return coordinates.AsGameCoordinate(Camera);
        }

        public void StartDialog(Dialog dialog) {
            ChangeState(new DialogState(Game, dialog, _windowSize, Camera, _spriteStore));
        }
    }
}

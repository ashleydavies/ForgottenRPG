using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using SFML.Graphics;
using SFML.Window;
using ShaRPG.GameState;
using ShaRPG.Items;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;
using Mouse = ShaRPG.Util.Mouse;

namespace ShaRPG {
    public class Game {
        private readonly DeltaClock _deltaClock;
        private readonly Stack<AbstractGameState> _gameStates;
        private readonly MapTileStore _mapTileStore;
        private readonly IRenderSurface _renderSurface;
        private readonly ISpriteStoreService _spriteStore;
        private readonly ItemManager _itemManager;
        private readonly RenderWindow _window;

        private Game() {
            string xmlDirectory = Path.Combine("Resources", "Data", "XML");
            
            ServiceLocator.LogService = new ConsoleLogService();

            _gameStates = new Stack<AbstractGameState>();

            _deltaClock = new DeltaClock();
            _spriteStore = new CachedFileSpriteStoreService(Path.Combine("Resources", "Image"));
            _mapTileStore = new MapTileStore(xmlDirectory, _spriteStore);
            _itemManager = new ItemManager(xmlDirectory);

            _window = new RenderWindow(VideoMode.FullscreenModes[0], "RPG", Styles.Titlebar | Styles.Fullscreen);
            _renderSurface = new WindowRenderSurface(_window);
            Mouse.SFMLWindow = _window;
            Vector2I windowSize = new Vector2I((int) _window.Size.X, (int) _window.Size.Y);

            _window.Closed += (sender, args)
                => _window.Close();
            _window.MouseWheelMoved += (sender, args)
                => _gameStates.Peek().MouseWheelMoved(args.Delta);
            _window.MouseButtonReleased += (sender, args)
                => _gameStates.Peek().Clicked(new ScreenCoordinate(args.X, args.Y));
            _window.Resized += (sender, args)
                => _renderSurface.Size = windowSize;

            SetGameState(new StateGame(this, windowSize, _spriteStore, _mapTileStore));

            while (_window.IsOpen) {
                _window.DispatchEvents();
                _window.Clear();

                _renderSurface.SetCamera(_gameStates.Peek().Camera);

                try {
                    _gameStates.Peek().Update(_deltaClock.GetDelta());

                    foreach (var state in (_gameStates.ToList() as IEnumerable<AbstractGameState>).Reverse()) {
                        state.Render(_renderSurface);
                    }
                } catch (EndGameException) {
                    break;
                }

                _window.Display();
            }

            _window.Close();
        }

        public void SetGameState(AbstractGameState state) {
            _gameStates.Clear();
            _gameStates.Push(state);
        }

        public void ChangeGameState(AbstractGameState state) {
            _gameStates.Push(state);
        }

        public void EndGameState() {
            _gameStates.Pop();
        }

        public static void Main(string[] args) {
            new Game();
        }
    }
}

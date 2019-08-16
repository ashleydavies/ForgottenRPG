using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.Entity;
using ShaRPG.GameState;
using ShaRPG.Items;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG {
    public class Game {
        private readonly DeltaClock _deltaClock;
        private readonly Stack<AbstractGameState> _gameStates;
        private readonly MapTileStore _mapTileStore;
        private readonly ITextureStore _textureStore;
        private readonly ItemManager _itemManager;
        private readonly RenderWindow _window;

        private Game() {
            string xmlDirectory = Path.Combine("Resources", "Data", "XML");
            
            ServiceLocator.LogService = new ConsoleLogService();

            _gameStates = new Stack<AbstractGameState>();

            _deltaClock = new DeltaClock();
            _textureStore = new CachedFileTextureStore(Path.Combine("Resources", "Image"));
            _mapTileStore = new MapTileStore(xmlDirectory, _textureStore);
            _itemManager = new ItemManager(xmlDirectory, _textureStore);

            _window = new RenderWindow(VideoMode.FullscreenModes[0], "RPG", Styles.Titlebar | Styles.Fullscreen);
            Vector2f windowSize = new Vector2f(_window.Size.X, _window.Size.Y);

            _window.Closed += (sender, args)
                => _window.Close();
            _window.MouseWheelScrolled += (sender, args)
                => _gameStates.Peek().MouseWheelMoved(args.Delta);
            _window.MouseButtonReleased += (sender, args)
                => _gameStates.Peek().Clicked(new ScreenCoordinate(args.X, args.Y));

            try {
                SetGameState(new StateGame(this, windowSize, _textureStore, _mapTileStore, _itemManager));
            } catch (EntityException e) {
                SetGameState(new ExceptionState(this, e, windowSize, _textureStore));
            }

            while (_window.IsOpen) {
                _window.DispatchEvents();
                _window.Clear();

                try {
                    _gameStates.Peek().Update(_deltaClock.GetDelta());

                    foreach (var state in (_gameStates.ToList() as IEnumerable<AbstractGameState>).Reverse()) {
                        state.Render(_window);
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

        public void ShowMouse() {
            _window.SetMouseCursorVisible(true);
        }

        public void HideMouse() {
            _window.SetMouseCursorVisible(false);
        }

        public ScreenCoordinate MousePosition => new ScreenCoordinate(Mouse.GetPosition(_window));

        public static void Main(string[] args) {
            new Game();
        }
    }
}

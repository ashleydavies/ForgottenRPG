using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ForgottenRPG.Entity;
using ForgottenRPG.GameState;
using ForgottenRPG.Items;
using ForgottenRPG.Map;
using ForgottenRPG.Service;
using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ForgottenRPG {
    public record struct GameUtils(RenderWindow window, MapTileStore mapTileStore, ITextureStore textureStore, ItemManager itemManager) { }
    public class Game {
        private readonly DeltaClock _deltaClock;
        private readonly Stack<AbstractGameState> _gameStates;
        private readonly RenderWindow _window;
        private readonly GameUtils _utils;

        private Game() {
            string xmlDirectory = Path.Combine("Resources", "Data", "XML");
            
            ServiceLocator.LogService = new ConsoleLogService();

            _gameStates = new Stack<AbstractGameState>();

            _deltaClock = new DeltaClock();
            
            var textureStore = new CachedFileTextureStore(Path.Combine("Resources", "Image"));
            var mapTileStore = new MapTileStore(xmlDirectory, textureStore);
            var itemManager  = new ItemManager(xmlDirectory, textureStore);

            // _window = new RenderWindow(VideoMode.DesktopMode, "RPG", Styles.Titlebar | Styles.Fullscreen);
            _window = new RenderWindow(VideoMode.DesktopMode, "RPG");
            
            _utils  = new GameUtils(_window, mapTileStore, textureStore, itemManager);
            
            Vector2f windowSize = new Vector2f(_window.Size.X, _window.Size.Y);

            _window.Closed += (sender, args)
                => _window.Close();
            _window.MouseWheelScrolled += (sender, args)
                => _gameStates.Peek().MouseWheelMoved(args.Delta);
            _window.MouseButtonReleased += (sender, args)
                => _gameStates.Peek().Clicked(new ScreenCoordinate(args.X, args.Y));

            try {
                // SetGameState(new StateGame(this, windowSize, _textureStore, _mapTileStore, _itemManager));
                SetGameState(new WorldMapState(_utils, this, xmlDirectory));
            } catch (EntityException e) {
                SetGameState(new ExceptionState(this, e, windowSize, _utils));
            }

            while (_window.IsOpen) {
                try {
                    _window.DispatchEvents();
                    _window.Clear();
                    _gameStates.Peek().Update(_deltaClock.GetDelta());

                    foreach (var state in (_gameStates.ToList() as IEnumerable<AbstractGameState>).Reverse()) {
                        state.Render(_window);
                    }
                } catch (EndGameException) {
                    break;
                } catch (Exception e) {
                    if (_gameStates.Peek() is ExceptionState) throw;
;                    SetGameState(new ExceptionState(this, e, windowSize, _utils));
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

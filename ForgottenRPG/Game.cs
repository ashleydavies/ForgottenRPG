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
            ServiceLocator.StdlibBytes = LoadStdlibBytes();

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

        private static List<uint> LoadStdlibBytes() {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "Data", "Scripts", "Compiled", "stdlib",
                                    "stdlib.fscrbyte");
            if (!File.Exists(path)) {
                throw new InvalidOperationException($"Missing stdlib bytecode: {path}");
            }

            var contents = File.ReadAllText(path).Trim();
            if (string.IsNullOrWhiteSpace(contents)) {
                throw new InvalidOperationException($"Stdlib bytecode is empty: {path}");
            }

            var bytes = contents.Split(',').Select(uint.Parse).ToList();
            ServiceLocator.LogService.Log(LogType.Info, $"Stdlib bytecode loaded: {path}");
            ServiceLocator.LogService.Log(LogType.Info, $"Stdlib words: {bytes.Count}");
            if (bytes.Count >= 5) {
                ServiceLocator.LogService.Log(LogType.Info, $"Stdlib entry: {bytes[0]} staticEnd: {bytes[1]} relocOff: {bytes[2]} exportOff: {bytes[3]} baseRelocOff: {bytes[4]}");
                LogStdlibExports(bytes);
            } else {
                ServiceLocator.LogService.Log(LogType.Info, "Stdlib header too small to read offsets.");
            }
            return bytes;
        }

        private static void LogStdlibExports(List<uint> bytes) {
            uint exportOffset = bytes[3];
            if (exportOffset >= bytes.Count) {
                ServiceLocator.LogService.Log(LogType.Info, "Stdlib export table offset out of range.");
                return;
            }

            int i = (int)exportOffset;
            uint count = bytes[i++];
            ServiceLocator.LogService.Log(LogType.Info, $"Stdlib exports: {count}");
            var sample = new List<uint>();
            for (uint e = 0; e < count && i + 1 < bytes.Count && sample.Count < 12; e++) {
                uint symbolHash = bytes[i++];
                i++; // address
                sample.Add(symbolHash);
            }
            if (sample.Count > 0) {
                ServiceLocator.LogService.Log(LogType.Info, $"Stdlib export sample: {string.Join(", ", sample)}");
            }
        }
    }
}

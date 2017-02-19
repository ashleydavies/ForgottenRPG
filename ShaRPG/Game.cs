#region

using System;
using System.Collections.Generic;
using System.IO;
using DataTypes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.GameState;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;

#endregion

namespace ShaRPG {
    public class Game {
        private readonly DeltaClock _deltaClock;
        private readonly Stack<AbstractGameState> _gameStates;
        private readonly MapTileStore _mapTileStore;
        private readonly IRenderSurface _renderSurface;
        private readonly ISpriteStoreService _spriteStore;
        private readonly RenderWindow _window;

        private Game() {
            ServiceLocator.LogService = new ConsoleLogService();

            _gameStates = new Stack<AbstractGameState>();

            _deltaClock = new DeltaClock();
            _spriteStore = new CachedFileSpriteStoreService(Path.Combine("Resources", "Image"));
            _mapTileStore = new MapTileStore(Path.Combine("Resources", "Data", "XML"), _spriteStore);

            _window = new RenderWindow(new VideoMode(1800, 900), "RPG", Styles.Titlebar) {
                                                                                             Size =
                                                                                                 new Vector2u(1800, 900)
                                                                                         };
            _renderSurface = new WindowRenderSurface(_window);

            _window.Closed += (sender, args) => _window.Close();
            _window.Resized +=
                (sender, args) => _renderSurface.Size = new Vector2I((int) _window.Size.X, (int) _window.Size.Y);

            _window.MouseWheelMoved += (sender, args) => _gameStates.Peek().MouseWheelMoved(args.Delta);

            SetGameState(new GameState.GameState(this, _spriteStore, _mapTileStore));

            while (_window.IsOpen) {
                _window.DispatchEvents();
                _window.Clear();

                _renderSurface.SetCamera(_gameStates.Peek().Camera);

                try {
                    _gameStates.Peek().Update(_deltaClock.GetDelta());
                    _gameStates.Peek().Render(_renderSurface);
                } catch (EndGameException e) {
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

        public static void Main(string[] args) {
            var game = new Game();
        }
    }
}

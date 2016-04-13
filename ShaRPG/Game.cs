using System;
using System.Collections.Generic;
using System.IO;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.GameState;
using ShaRPG.Map;
using ShaRPG.Service;
using ShaRPG.Util;

namespace ShaRPG
{
    public class Game
    {
        private readonly RenderWindow _window;
        private readonly ISpriteStoreService _spriteStore;
        private readonly MapTileStore _mapTileStore;
        private readonly IRenderSurface _renderSurface;
        private readonly Stack<AbstractGameState> _gameStates;
        private readonly DeltaClock _deltaClock;

        public Game()
        {
            ServiceLocator.LogService = new ConsoleLogService();

            _gameStates = new Stack<AbstractGameState>();
            
            _deltaClock = new DeltaClock();
            _spriteStore = new CachedFileSpriteStoreService(Path.Combine("resources", "img"));
            _mapTileStore = new MapTileStore(Path.Combine("resources", "data", "xml"), _spriteStore);

            _window = new RenderWindow(new VideoMode(2400, 1600), "RPG", Styles.Titlebar) {Size = new Vector2u(2400, 1600)};
            _renderSurface = new WindowRenderSurface(_window);

            _window.Closed += (sender, args) => _window.Close();
            _window.Resized += (sender, args) => _renderSurface.Size = new Vector2I((int)_window.Size.X, (int)_window.Size.Y);

            SetGameState(new GameState.GameState(this, _spriteStore, _mapTileStore));

            while (_window.IsOpen)
            {
                _window.DispatchEvents();
                _window.Clear();

                _renderSurface.SetCamera(_gameStates.Peek().Camera);
                
                _gameStates.Peek().Update(_deltaClock.GetDelta());
                _gameStates.Peek().Draw(_renderSurface);

                _window.Display();
            }

            Console.ReadLine();
        }

        public void SetGameState(AbstractGameState state)
        {
            _gameStates.Clear();
            _gameStates.Push(state);
        }

        public static void Main(string[] args)
        {
            Game game = new Game();
        }
    }
}   
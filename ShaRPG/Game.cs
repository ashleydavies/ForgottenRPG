using System;
using System.IO;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ShaRPG.Service;

namespace ShaRPG
{
    public class Game
    {
        private readonly RenderWindow _window;

        public Game()
        {
            ISpriteStoreService spriteStore = new CachedFileSpriteStoreService(Path.Combine("resources", "img"));
            var sprite = spriteStore.GetSprite("water_1");

            _window = new RenderWindow(new VideoMode(1200, 800), "RPG", Styles.Titlebar) {Size = new Vector2u(1200, 800)};

            _window.Closed += (sender, args) => _window.Close();

            while (_window.IsOpen)
            {
                _window.DispatchEvents();
                _window.Clear();
                _window.Draw(sprite.UnderlyingSprite);
                _window.Display();
            }

            Console.ReadLine();
        }

        public static void Main(string[] args)
        {
            Game game = new Game();
        }
    }
}   
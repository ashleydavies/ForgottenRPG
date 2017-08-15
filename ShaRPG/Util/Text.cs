using SFML.Graphics;
using SFML.System;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class Text : IDrawable {
        public int Width => (int) _text.GetLocalBounds().Width;
        public int Height => (int) _text.GetLocalBounds().Height;
        private readonly SFML.Graphics.Text _text;

        public Text(Font font, string content, uint characterSize = 12) {
            _text = new SFML.Graphics.Text(content, font.UnderlyingFont) {
                Color = Color.Black,
                CharacterSize = characterSize
            };
        }

        public void Draw(RenderWindow window, GameCoordinate position) {
            _text.Position = new Vector2f(position.X, position.Y);
            window.Draw(_text);
        }

        public void Update(float delta) { }
    }
}

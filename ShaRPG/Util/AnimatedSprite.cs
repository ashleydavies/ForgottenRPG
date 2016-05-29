#region

using System.Collections.Generic;
using SFML.Graphics;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Util {
    internal class AnimatedSprite : IDrawable {
        private readonly List<Sprite> _sprites;
        private readonly double _switchLength;
        private int _currentSprite;
        private double _timer;

        public AnimatedSprite(List<Sprite> sprites, double switchLength) {
            _switchLength = switchLength;
            _timer = 0;
            _sprites = sprites;
        }

        private Sprite Sprite => _sprites[_currentSprite];

        public void Update(float delta) {
            _timer += delta;
            if (_timer > _switchLength) {
                _timer = 0;
                _currentSprite = (_currentSprite + 1) % _sprites.Count;
            }
        }

        public void Draw(RenderWindow window, GameCoordinate position) {
            Sprite.Draw(window, position);
        }
    }
}

using System.Collections.Generic;
using SFML.Graphics;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    class AnimatedSprite : IDrawable
    {
        private int _currentSprite = 0;
        private readonly List<Sprite> _sprites;
        private Sprite Sprite => _sprites[_currentSprite];
        private readonly double _switchLength;
        private double _timer;

        public AnimatedSprite(List<Sprite> sprites, double switchLength)
        {
            _switchLength = switchLength;
            _timer = 0;
            _sprites = sprites;
        }

        public void Update(float delta)
        {
            _timer += delta;
            if (_timer > _switchLength)
            {
                _timer = 0;
                _currentSprite = (_currentSprite + 1)%_sprites.Count;
            }
        }

        public void Draw(RenderWindow window, GameCoordinate position)
        {
            Sprite.Draw(window, position);
        }
    }
}

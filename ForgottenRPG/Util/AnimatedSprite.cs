#region

using System.Collections.Generic;
using SFML.Graphics;

#endregion

namespace ForgottenRPG.Util {
    internal class AnimatedSprite : ISpriteable {
        public Sprite Sprite => _sprites[_currentSprite];
        private readonly List<Sprite> _sprites;
        private readonly double _switchLength;
        private int _currentSprite;
        private double _timer;

        public AnimatedSprite(List<Sprite> sprites, double switchLength) {
            _switchLength = switchLength;
            _timer = 0;
            _sprites = sprites;
        }

        public void Update(float delta) {
            _timer += delta;
            if (_timer > _switchLength) {
                _timer = 0;
                _currentSprite = (_currentSprite + 1) % _sprites.Count;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;

namespace ShaRPG.Util {
    public class FpsCounter {
        private float _time = 0.0f;
        private List<float> _deltas = new List<float>();
        private float Fps => _deltas.Count;

        public void Update(float delta) {
            _time += delta;
            _deltas.Add(_time);
            _deltas = _deltas.SkipWhile(x => _time > x + 1.0f).ToList();
        }

        public void Render(RenderTarget renderSurface) {
            renderSurface.Draw(new Text($"FPS: {Fps}", Config.GuiFont) {
                Position = new Vector2f(0, 0),
                CharacterSize = 16,
                Color = Color.White
            });
        }
    }
}

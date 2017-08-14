using System.Collections.Generic;
using System.Linq;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class FPSCounter {
        private float _time = 0.0f;
        private List<float> _deltas = new List<float>();
        private float _fps => _deltas.Count;

        public void Update(float delta) {
            _time += delta;
            _deltas.Add(_time);
            _deltas = _deltas.SkipWhile(x => _time > x + 1.0f).ToList();
        }
        
        public void Render(IRenderSurface renderSurface) {
            renderSurface.Render(new Text(Config.GuiFont, $"FPS: {_fps}"), new ScreenCoordinate(0, 0));
        }
    }
}

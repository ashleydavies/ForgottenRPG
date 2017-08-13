using System;

namespace ShaRPG.Util {
    internal class RenderSurfaceOffset : IDisposable {
        private readonly IRenderSurface _renderSurface;
        private readonly Vector2I _offset;

        public RenderSurfaceOffset(IRenderSurface surface, Vector2I offset) {
            _renderSurface = surface;
            _offset = offset;
            _renderSurface.AddRenderOffset(_offset);
        }

        public void Dispose() {
            _renderSurface.SubtractRenderOffset(_offset);
        }
    }
}

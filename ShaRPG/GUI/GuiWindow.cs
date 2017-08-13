using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public class GuiWindow {
        private const int BorderTSize = 12;
        private const int BgTSize = 60;
        private readonly ISpriteStoreService _spriteStore;
        private readonly Vector2I _center;
        private readonly Vector2I _size;
        private Vector2I Position => _center - _size / 2;
        private Vector2I BgPos => Position + new Vector2I(BorderTSize, BorderTSize);
        private readonly List<IGuiComponent> _components = new List<IGuiComponent>();

        private ScreenCoordinate BackgroundTilePos(int x, int y) =>
            new ScreenCoordinate(BgPos + new Vector2I(x * BgTSize, y * BgTSize));

        private ScreenCoordinate TopBorderPos(int n) =>
            new ScreenCoordinate(Position.X + n * BorderTSize, Position.Y);

        private ScreenCoordinate BottomBorderPos(int n) =>
            new ScreenCoordinate(Position.X + n * BorderTSize,
                                 Position.Y + (SideSegments - 1) * BorderTSize);

        private ScreenCoordinate LeftBorderPos(int n) =>
            new ScreenCoordinate(Position.X, Position.Y + n * BorderTSize);

        private ScreenCoordinate RightBorderPos(int n) =>
            new ScreenCoordinate(Position.X + (TopBottomSegments - 1) * BorderTSize,
                                 Position.Y + n * BorderTSize);

        private int TopBottomSegments => Math.Max(_size.X / BorderTSize, 0);
        private int SideSegments => Math.Max(_size.Y / BorderTSize, 0);
        private int BgXSegments => Math.Max((_size.X - BorderTSize * 2) / 60, 0);
        private int BgYSegments => Math.Max((_size.Y - BorderTSize * 2) / 60, 0);

        public GuiWindow(ISpriteStoreService spriteStore, Vector2I center, Vector2I size) {
            if ((size.X - 2 * BorderTSize) % BgTSize != 0 || (size.Y - 2 * BorderTSize) % BgTSize != 0)
                throw new GuiException("Bad size for GUI window");

            _spriteStore = spriteStore;
            _center = center;
            _size = size;
        }

        public void Render(IRenderSurface renderSurface) {
            RenderUi(renderSurface, "corner_tl", TopBorderPos(0));
            RenderUi(renderSurface, "corner_tr", TopBorderPos(TopBottomSegments - 1));
            RenderUi(renderSurface, "corner_bl", LeftBorderPos(SideSegments - 1));
            RenderUi(renderSurface, "corner_br", RightBorderPos(SideSegments - 1));

            for (int t = 1; t < TopBottomSegments - 1; t++) {
                RenderUi(renderSurface, "side_t", TopBorderPos(t));
                RenderUi(renderSurface, "side_b", BottomBorderPos(t));
            }

            for (int t = 1; t < SideSegments - 1; t++) {
                RenderUi(renderSurface, "side_l", LeftBorderPos(t));
                RenderUi(renderSurface, "side_r", RightBorderPos(t));
            }

            for (int y = 0; y < BgYSegments; y++) {
                for (int x = 0; x < BgXSegments; x++) {
                    RenderUi(renderSurface, "background", BackgroundTilePos(x, y));
                }
            }

            using (new RenderSurfaceOffset(renderSurface, BgPos)) {
                _components.ForEach(component => component.Render(renderSurface));
            }
        }

        protected void AddComponent(IGuiComponent component) {
            _components.Add(component);
        }

        private void RenderUi(IRenderSurface renderSurface, string part, ScreenCoordinate position) {
            renderSurface.Render(_spriteStore.GetSprite($"ui_{part}"), position);
        }
    }
}

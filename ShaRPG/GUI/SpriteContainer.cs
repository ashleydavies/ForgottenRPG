using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public class SpriteContainer : AbstractGuiComponent {
        public override int Height => Sprite.Height;
        public override int Width => Sprite.Width;
        public Sprite Sprite;
        private readonly Alignment _alignment;

        private ScreenCoordinate position => _alignment == Alignment.Left
                                                 ? new ScreenCoordinate(0, 0)
                                                 : (_alignment == Alignment.Right
                                                        ? new ScreenCoordinate(Parent.Width - Width, 0)
                                                        : new ScreenCoordinate(Parent.Width / 2 - Width / 2, 0));

        public SpriteContainer(Sprite sprite, Alignment alignment = Alignment.Left) {
            Sprite = sprite;
            _alignment = alignment;
        }

        public override void Render(IRenderSurface renderSurface) {
            renderSurface.Render(Sprite, position);
        }

        public override void Reflow() { }
    }
}

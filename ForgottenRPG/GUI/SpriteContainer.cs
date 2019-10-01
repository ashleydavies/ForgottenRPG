using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.GUI {
    public class SpriteContainer : AbstractGuiComponent {
        public override int Height => Sprite.TextureRect.Height;
        public override int Width => Sprite.TextureRect.Width;
        public Sprite Sprite;
        private readonly Alignment _alignment;

        public SpriteContainer(Sprite sprite, Alignment alignment = Alignment.Left) {
            Sprite = sprite;
            _alignment = alignment;
        }

        public override void Render(RenderTarget renderSurface) {
            renderSurface.Draw(Sprite);
        }

        public override void Reflow() {
            SetPosition();
        }

        private void SetPosition() {
            Sprite.Position = _alignment == Alignment.Left
                                  ? new ScreenCoordinate(0, 0)
                                  : (_alignment == Alignment.Right
                                         ? new ScreenCoordinate(Parent.Width - Width, 0)
                                         : new ScreenCoordinate(Parent.Width / 2 - Width / 2, 0));
        }
    }
}

using SFML.Graphics;
using SFML.System;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Util {
    public class Sprite : IDrawable {
        public static readonly Sprite Null = new NullSprite();
        public readonly SFML.Graphics.Sprite UnderlyingSprite;
        public int Height => this != Null ? UnderlyingSprite.TextureRect.Height : 0;
        public int Width => this != Null ? UnderlyingSprite.TextureRect.Width : 0;

        public Sprite(Texture texture, int x, int y, int width, int height) {
            UnderlyingSprite = new SFML.Graphics.Sprite {
                Texture = texture.UnderlyingTexture,
                TextureRect = new IntRect(x, y, width, height)
            };
        }

        private Sprite() { }

        public virtual void Draw(RenderWindow window, GameCoordinate position) {
            UnderlyingSprite.Position = new Vector2f(position.X, position.Y);
            window.Draw(UnderlyingSprite);
        }

        private class NullSprite : Sprite {
            public NullSprite() : base() { }

            public override void Draw(RenderWindow window, GameCoordinate position) { }
        }

        public void Update(float delta) { }
    }
}

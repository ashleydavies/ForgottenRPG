using ForgottenRPG.Util;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.Map {
    public class MapTile {
        // Constants
        public const int Width = 64;
        public const int Height = 32;
        public const int HalfWidth = Width / 2;
        public const int HalfHeight = Height / 2;
        public static readonly MapTile Null = new NullMapTile(0, null, "Null Tile", false);
        private readonly ISpriteable _spriteable;
        public readonly bool Collideable;
        // Instance fields
        public readonly int Id;
        public readonly string Name;
        public readonly bool AutoFloor;

        internal MapTile(int id, ISpriteable spriteable, string name, bool collideable, bool autoFloor) {
            Id          = id;
            _spriteable = spriteable;
            Name        = name;
            Collideable = collideable;
            AutoFloor   = autoFloor;
        }

        public GameCoordinate TextureOffset { get; set; } = new GameCoordinate(0, 0);

        public virtual void Update(float delta) {
            _spriteable.Update(delta);
        }

        public virtual void Draw(RenderTarget renderSurface, TileCoordinate position) {
            Sprite sprite = _spriteable.Sprite;
            sprite.Position = (GameCoordinate) position + TextureOffset;
            renderSurface.Draw(sprite);
        }

        private class NullMapTile : MapTile {
            public NullMapTile(int id, ISpriteable spriteable, string name, bool collideable)
                : base(id, spriteable, name, collideable, false) { }

            public override void Update(float delta) { }
            public override void Draw(RenderTarget renderSurface, TileCoordinate position) { }
        }
    }
}

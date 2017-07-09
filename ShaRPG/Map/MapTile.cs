using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map {
    public class MapTile {
        // Constants
        public const int Width = 64;
        public const int Height = 32;
        public const int HalfWidth = Width / 2;
        public const int HalfHeight = Height / 2;
        public static readonly MapTile Null = new NullMapTile(0, null, "Null Tile", false);
        private readonly IDrawable _sprite;
        public readonly bool Collideable;
        // Instance fields
        public readonly int Id;
        public readonly string Name;

        internal MapTile(int id, IDrawable sprite, string name, bool collideable) {
            Id = id;
            _sprite = sprite;
            Name = name;
            Collideable = collideable;
        }

        public GameCoordinate TextureOffset { get; set; } = new GameCoordinate(0, 0);

        public virtual void Update(float delta) {
            _sprite.Update(delta);
        }

        public virtual void Draw(IRenderSurface renderSurface, TileCoordinate position) {
            renderSurface.Render(_sprite, (GameCoordinate) position + TextureOffset);
        }

        private class NullMapTile : MapTile {
            public NullMapTile(int id, IDrawable sprite, string name, bool collideable)
                : base(id, sprite, name, collideable) { }

            public override void Update(float delta) { }
            public override void Draw(IRenderSurface renderSurface, TileCoordinate position) { }
        }
    }
}

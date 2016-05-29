#region

using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

#endregion

namespace ShaRPG.Map {
    public class MapTile {
        // Constants
        public const int Width = 64;
        public const int Height = 32;
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

        public void Update(float delta) {
            _sprite.Update(delta);
        }

        public void Draw(IRenderSurface renderSurface, TileCoordinate position) {
            renderSurface.Render(_sprite, position + TextureOffset);
        }
    }
}

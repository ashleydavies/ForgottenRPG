using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShaRPG.Service;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.Map
{
    public class MapTile
    {
        // Constants
        public const int Width = 64;
        public const int Height = 32;

        // Instance fields
        public readonly int Id;
        public readonly string Name;
        public readonly bool Collideable;
        private readonly IDrawable _sprite;

        internal MapTile(int id, IDrawable sprite, string name, bool collideable)
        {
            Id = id;
            _sprite = sprite;
            Name = name;
            Collideable = collideable;
        }

        public void Update(float delta)
        {
            _sprite.Update(delta);
        }

        public void Draw(IRenderSurface renderSurface, TileCoordinate position)
        {
            renderSurface.Render(_sprite, position);
        }
    }
}
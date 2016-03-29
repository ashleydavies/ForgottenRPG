using System.Collections.Generic;
using ShaRPG.Service;

namespace ShaRPG.Map
{
    public class MapTile
    {
        // Constants
        public const int Width = 64;
        public const int Height = 32;
        public static List<MapTile> Tiles { get; } = new List<MapTile>();

        // Instance fields
        public readonly int Id;
        public readonly string Name;
        public readonly bool Collideable;

        private MapTile(int id, string name, bool collideable)
        {
            Id = id;
            Name = name;
            Collideable = collideable;
        }

        public static MapTile GetTile(int id)
        {
            foreach (var tile in Tiles)
            {
                if (tile.Id == id)
                {
                    return tile;
                }
            }

            ServiceLocator.LogService.Log(LogType.Warning, "Attempt to index non-existent tile " + id);
            return null;
        }


        public class MapTileBuilder
        {
            public readonly int Id;
            public string Name;
            public bool Collideable;

            public MapTileBuilder(int id)
            {
                Id = id;
            }

            public void Build()
            {
                Tiles.Add(new MapTile(Id, Name, Collideable));
            }

            public MapTileBuilder SetName(string name)
            {
                Name = name;
                return this;
            }

            public MapTileBuilder SetCollideable(bool collideable)
            {
                Collideable = collideable;
                return this;
            }
        }
    }
}
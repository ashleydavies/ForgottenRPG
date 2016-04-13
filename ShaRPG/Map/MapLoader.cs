using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ShaRPG.Util;

namespace ShaRPG.Map {
    class MapLoader
    {
        private readonly string _directory;
        private readonly MapTileStore _tileStore;

        public MapLoader(string directory, MapTileStore tileStore)
        {
            _directory = directory;
            _tileStore = tileStore;
        }

        public GameMap LoadMap(int id)
        {
            XDocument document;

            using (var fs = File.OpenRead(Path.Combine(_directory, id.ToString() + ".xml")))
            {
                document = XDocument.Load(fs);
            }

            var layerData = document
                .Elements("Map")
                .Elements("Tiles")
                .Elements("Layer");

            var layer = layerData.FirstOrDefault();

            if (layer == null)
            {
                return null;
            }

            var rows = layer.Elements("Row");

            int[][] tiles = new int[rows.Count()][];

            int y = 0, x = 0;

            foreach (var cols in rows.Select(row => row.Elements("Tile")))
            {
                tiles[y] = new int[cols.Count()];

                x = 0;
                foreach (var col in cols)
                {
                    tiles[y][x] = int.Parse(col.Attribute("tileID").Value);
                    x++;
                }

                y++;
            }

            return new GameMap(tiles, new Vector2I(x, y), _tileStore);
        }
    }
}
